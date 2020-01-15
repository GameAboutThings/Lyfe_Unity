using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instrument : MonoBehaviour
{
    AudioSource[] audioSources;
    AudioClip[] audioClips;

    Meta_MusicGenerator.EInstrument instrument;
    Meta_MusicGenerator.EInstrumenType type;
    Meta_MusicGenerator.EKey key;
    Meta_MusicGenerator.EMode mode;
    bool play;
    int bpm;
    float secondsPerQuarter;

    Pattern pattern;

    int currentMeasure;
    int currentBeat;
    int currentsixteenthNote;
    float secondsPast;

    int sourcePointer;

    public void InitInstrument(Meta_MusicGenerator.EInstrument _instrument, Meta_MusicGenerator.EInstrumenType _type, Meta_MusicGenerator.EKey _key, Meta_MusicGenerator.EMode _mode, int _bpm)
    {
        play = false;

        audioClips = Meta_MusicGenerator.GetScaleAudio(_key, _mode, _instrument);
        instrument = _instrument;
        type = _type;
        key = _key;
        mode = _mode;
        bpm = _bpm;

        secondsPerQuarter = Meta_MusicGenerator.GetDurationOfOneBeat(bpm);
        audioSources = new AudioSource[5]; //5 should be enough; there will rarely be more notes playing on top of each other
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
        }
    }
 
    void Update()
    {
        if (!play)
            return;

        PlayPattern();
    }

    public void SetPattern(Pattern _pattern)
    {
        pattern = _pattern;
    }


    private void PlayPattern()
    {
        if (pattern == null)
            return;

        PlayMeasure(currentMeasure);
    }

    private void PlayMeasure(int _measure)
    {
        PlayBeat(currentBeat);
    }

    private void PlayBeat(int _beat)
    {
        PlaySixteenth(currentsixteenthNote);
    }

    private void PlaySixteenth(int _sixteenth)
    {
        secondsPast += Time.deltaTime;

        if (secondsPast < secondsPerQuarter / 4)
            return;

        int index = currentBeat * pattern.GetMeasure(currentMeasure).GetSixteenthsPerBeat() + currentsixteenthNote;
        Pattern.Measure.Note curNote = pattern.GetMeasure(currentMeasure).GetNote(index);

        if (curNote != null)
        {
            int noteIndex = curNote.GetStep() - 1;

            if (noteIndex > audioClips.Length - 1 || noteIndex < 0)
                noteIndex = 0;
            Debug.Log(audioClips);
            Debug.Log(audioSources);
            audioSources[sourcePointer].clip = audioClips[noteIndex];
            //TODO multiply frequency for octave
            audioSources[sourcePointer].Play();
            sourcePointer = (sourcePointer + 1) % audioSources.Length;
        }

        IncreaseCurrentSixteenth();
        secondsPast = 0;
    }

    private void IncreaseCurrentSixteenth()
    {
        currentsixteenthNote = (currentsixteenthNote + 1) % pattern.GetMeasure(currentMeasure).GetSixteenthsPerBeat();
        if (currentsixteenthNote == 0)
        {
            currentBeat = (currentBeat + 1) % pattern.GetMeasure(currentMeasure).GetBeats();
            if (currentBeat == 0)
            {
                currentMeasure = (currentMeasure + 1) / pattern.GetMeasures();
            }
        }
    }

    public void Play()
    {
        play = true;
    }

    public void Stop()
    {
        play = false;
    }
}
