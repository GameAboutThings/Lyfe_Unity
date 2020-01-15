using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meta_MusicGenerator
{
    public enum EBeats
    {
        eDownbeat,
        eBackbeat
    };

    public enum EInstrumenType
    {
        eMelody,
        eHarmony,
        eRhythm,
        ePercussion
    }

    public enum EKey
    {
        A,
        As,
        B,
        C,
        Cs,
        D,
        Ds,
        E,
        F,
        Fs,
        G,
        Gs
    }

    public enum EMode
    {
        minor,
        major
    }

    public enum EInstrument
    {
        bassDrum,
        synthWarm
    }

    public static AudioClip[] GetScaleAudio(EKey _key, EMode _mode, EInstrument _instrument)
    {
        AudioClip[] scaleTones = new AudioClip[7];
        scaleTones[0] = GetAudioClipInstrument(_key, _mode, _instrument, 1);
        for (int i = 1; i < 7; i++)
        {
            scaleTones[i] = GetAudioClipInstrument(_key, _mode, _instrument, i + 1);
        }
        return scaleTones;
    }

    public static int GetStepsizeForMode(int _step, EMode _mode)
    {
        int stepsize = 2;
        switch (_mode)
        {
            case EMode.minor:
                stepsize = GetStepsizeMinor(_step);
                break;
            case EMode.major:
                stepsize = GetStepsizeMajor(_step);
                break;
        }
        return stepsize;
    }

    private static int GetStepsizeMinor(int _step)
    {
        int stepsize = 2;

        if (_step == 3 || _step == 6)
        {
            stepsize = 1;
        }

        return stepsize;
    }

    private static int GetStepsizeMajor(int _step)
    {
        int stepsize = 2;

        if (_step == 4 || _step == 8)
        {
            stepsize = 1;
        }

        return stepsize;
    }

    private static EKey AddStep(EKey _note, int _stepSize)
    {
        int note = (int)_note;
        note = (note + _stepSize) % 12;
        return (EKey)note;
    }

    private static EKey GetNote(EKey _key, EMode _mode, int _step)
    {
        EKey note = _key;
        //1 is root
        for (int i = 2; i < _step; i++)
        {
            note = AddStep(note, GetStepsizeForMode(_step, _mode));
        }
        return note;
    }

    private static AudioClip GetAudioClipInstrument(EKey _key, EMode _mode, EInstrument _instrument, int _step)
    {
        string clipString = "";

        switch (_instrument)
        {
            case EInstrument.bassDrum:
                return Resources.Load<AudioClip>("Audio/Music/Test/bassDrum");
            case EInstrument.synthWarm:
                clipString += "synthWarm";
                break;
        }

        return Resources.Load<AudioClip>("Audio/Music/Test/" + clipString);
    }

    public static float GetDurationOfOneBeat(int _bpm)
    {
        float beatsPerSecond = _bpm / 60f;
        float secondsPerBeat = 1f / beatsPerSecond;

        return secondsPerBeat;
    }

    public static int GetBPM(Seed _seed)
    {
        //always take position 12
        return 80 - 10 + (Seed.allowedChars.IndexOf(_seed.GetSeedString()[12]) % 20) ;
    }
}
