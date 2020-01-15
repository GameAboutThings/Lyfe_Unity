using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicGenerator : MonoBehaviour
{
    [SerializeField]
    GameObject instrumentPrefab;

    Seed seed;
    int bpm;
    float secondsPerQuarter;
    float secondsPast;

    int currentMeasureTotal;
    Pattern[] currentPatterns;
    bool patternDistributed;

    //INSTRUMENTS
    Instrument rhythm;
    GameObject rhythmObject;

    void Start()
    {
        seed = new Seed();
        bpm = Meta_MusicGenerator.GetBPM(seed);
        secondsPerQuarter = Meta_MusicGenerator.GetDurationOfOneBeat(bpm);

        patternDistributed = true;

        rhythmObject = Instantiate(instrumentPrefab, transform.position, Quaternion.identity);
        rhythm = rhythmObject.GetComponent<Instrument>();
        rhythm.InitInstrument(Meta_MusicGenerator.EInstrument.bassDrum, Meta_MusicGenerator.EInstrumenType.ePercussion, Meta_MusicGenerator.EKey.A, Meta_MusicGenerator.EMode.minor, bpm);
        
    }

    void Update()
    {
        DistributeNewPatterns();
    }

    private void DistributeNewPatterns()
    {
        if (currentPatterns != null)
        {
            //distribute the pattern
            DistributePatterns();            
        }
        else
        {
            //wait until the pattern is distributed and then generate a new one
            if (!patternDistributed)
                return;

            GeneratePatterns();
        }
    }

    private void DistributePatterns()
    {
        secondsPast += Time.deltaTime;

        if (secondsPast < secondsPerQuarter * 4 * 4)
            return;

        rhythm.SetPattern(currentPatterns[0]);
        //rhythm.Play();


        patternDistributed = true;
        currentPatterns = null;
        secondsPast = 0;
    }

    private void GeneratePatterns()
    {
        currentPatterns = new Pattern[1];
        currentPatterns[0] = new Pattern();
        

        for (int i = 0; i < 4; i++)
        {
            int beats = GetNumBeats(currentMeasureTotal, seed);
            int notesPerBeat = GetNumNotesPerBeat(currentMeasureTotal, seed);
            currentMeasureTotal++;

            currentPatterns[0].SetMeasure(i, new Pattern.Measure(beats, notesPerBeat, GenerateMeasureRhythm(currentMeasureTotal, beats, notesPerBeat, seed)));
        }

        patternDistributed = false;
    }

    private int GetNumBeats(int _measure, Seed _seed)
    {
        return 4;
    }

    private int GetNumNotesPerBeat(int _measure, Seed _seed)
    {
        return 4;
    }

    private bool[] GenerateMeasureRhythm(int _measure, int _beats, int _notesPerBeat, Seed _seed)
    {
        bool[] rhythm = new bool[_beats * _notesPerBeat];

        for (int b = 0; b < _beats; b++)
        {
            for (int n = 0; n < _notesPerBeat; n++)
            {
                float chance = 0f;

                //first beat
                if (b == 0)
                {
                    //first note
                    if (n == 0)
                    {
                        //100%
                        chance = 1f;
                    }
                    else if (n == _notesPerBeat - 1)
                    {
                        //50%
                        chance = 0.33f;
                    }
                    //strong note
                    else if (n % 2 == 0)
                    {
                        //33%
                        chance = 0.2f;
                    }
                    //weak note
                    else
                    {
                        //5%
                        chance = 0.05f;
                    }
                }
                //strong beats
                else if (b % 2 == 0)
                {
                    //first note
                    if (n == 0)
                    {
                        chance = 0.7f;
                    }
                    else if (n == _notesPerBeat - 1)
                    {
                        chance = 0.3f;
                    }
                    //strong note
                    else if (n % 2 == 0)
                    {
                        chance = 0.2f;
                    }
                    //weak note
                    else
                    {
                        chance = 0.05f;
                    }
                }
                //weak beats
                else
                {
                    //first note
                    if (n == 0)
                    {
                        chance = 0.5f;
                    }
                    else if (n == _notesPerBeat - 1)
                    {
                        chance = 0.3f;
                    }
                    //strong note
                    else if (n % 2 == 0)
                    {
                        chance = 0.1f;
                    }
                    //weak note
                    else
                    {
                        chance = 0.01f;
                    }
                }
                rhythm[GetIndex(b, n, _notesPerBeat)] = _seed.GetSeed()[(currentMeasureTotal + b + n) % _seed.GetSeed().Length] < (Seed.allowedChars.Length * chance);
            }
        }

        return rhythm;
    }

    private int GetIndex(int _beat, int _note, int _notesPerBeat)
    {
        return _beat * _notesPerBeat + _note;
    }
}
