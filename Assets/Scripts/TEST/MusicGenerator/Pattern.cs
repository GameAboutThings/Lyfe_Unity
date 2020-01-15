using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pattern
{
    Measure[] measures;

    public Pattern()
    {
        measures = new Measure[4];
    }

    public int GetMeasures()
    {
        return measures.Length;
    }

    public void SetMeasure(int _index, Measure _measure)
    {
        if (_index >= measures.Length)
            Debug.Log("Can't insert into measure array. Out of bounds.");
        else
            measures[_index] = _measure;
    }

    public Measure GetMeasure(int _index)
    {
        if (_index >= measures.Length)
            return null;

        return measures[_index];
    }

    public class Measure
    {
        Note[] notes;
        int sixteenthsPerBeat;
        int beats;

        //public Measure(int _beats, int _sixteenthsPerBeat)
        //{
        //    beats = _beats;
        //    sixteenthsPerBeat = _sixteenthsPerBeat;
        //    notes = new Note[sixteenthsPerBeat * beats];
        //}

        //Generate a rhythmic pattern purely from the root note
        public Measure(int _beats, int _sixteenthsPerBeat, bool[] _hits)
        {
            beats = _beats;
            sixteenthsPerBeat = _sixteenthsPerBeat;
            notes = new Note[sixteenthsPerBeat * beats];

            for (int i = 0; i < _hits.Length; i++)
            {
                if (_hits[i])
                {
                    notes[i] = new Note(1, 1, 1);
                }
            }
        }

        public int GetSixteenthsPerBeat()
        {
            return sixteenthsPerBeat;
        }

        public int GetBeats()
        {
            return beats;
        }

        public Note GetNote(int _index)
        {
            if (_index >= notes.Length)
                return null;

            return notes[_index];
        }


        public class Note
        {
            int length; //length in 16th
            int step;
            int octave;

            public Note(int _step, int _octave, int _length)
            {
                length = _length;
                step = _step;
                octave = _octave;
            }

            public int GetStep()
            {
                return step;
            }

            public int GetOctave()
            {
                return octave;
            }

            public int GetLength()
            {
                return length;
            }
        }
    }
}


