using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshList<T>
{
    private Element<T> centerElement;

    /*
     * Sets the center of the MeshList.
     * If there already is a center this method will return false and the new center won't be set.
     */
    public bool SetCenter(T _centerElement)
    {
        if (centerElement != null)
            return false;

        centerElement = new Element<T>(_centerElement, 0, 0);
        return true;
    }

    private Element<T> GetElementAt(int _x, int _y, bool _throwException)
    {
        ArrayList queue = new ArrayList();

        Element<T> currentElement = centerElement;
        if (centerElement == null)
            return null;

        queue.Add(currentElement);

        Element<T> t;

        while (queue.Count != 0)
        {
            //get current element and remove it from the queue
            currentElement = (Element<T>)queue[0];
            queue.Remove(currentElement);
            //set its marker
            currentElement.SetMarker(true);

            System.Tuple<int, int> coord = currentElement.GetCoordinate();

            if (coord.Item1 == _x && coord.Item2 == _y)
            {
                SetAllMarkers(false);
                return currentElement;
            }             

            //get all neghbouring elements and add them to the queue
            t = currentElement.GetAbove();
            if (t != null && !t.GetMarker())
                queue.Add(t);
            t = currentElement.GetBelow();
            if (t != null && !t.GetMarker())
                queue.Add(t);
            t = currentElement.GetRight();
            if (t != null && !t.GetMarker())
                queue.Add(t);
            t = currentElement.GetLeft();
            if (t != null && !t.GetMarker())
                queue.Add(t);
        }

        SetAllMarkers(false);
        return null;
    }

    public T GetAt(int _x, int _y)
    {
        return GetElementAt(_x, _y, true).GetValue();
    }

    /*
     * Replaces the element at index (x, y) with the new element and returns the old one.
     */
    public T Replace(int _x, int _y, T _element)
    {
        Element<T> oldElement = GetElementAt(_x, _y, true);

        T oldValue = oldElement.GetValue();
        oldElement.SetValue(_element);
        return oldValue;
    }



    public void AddAbove(int _x, int _y, T _element)
    {
        Element<T> rootElement = GetElementAt(_x, _y, true); //if the element at the given index doesn't exist, the method will stop here

        if (rootElement == null)
            ThrowElementNotFoundAtIndex(_x, _y);

        if (rootElement.GetAbove() != null)
            ThrowSpaceTaken(_x, _y + 1, "Can't add above " + FormatCoordinates(_x, _y) + ".");

        //check surroundings next
        //there might already be another element here.
        //these 3 elements are the other ones that potentially surround the nem element
        Element<T> surAbove = GetElementAt(_x, _y + 2, false);
        Element<T> surRight = GetElementAt(_x + 1, _y + 1, false);
        Element<T> surLeft = GetElementAt(_x - 1, _y + 1, false);

        //check if the element above my new element already exists
        //and if so, whether it has a below element
        //if both is true, stop here
        if (surAbove != null)
            if(surAbove.GetBelow() != null)
                ThrowSpaceTaken(_x, _y + 1, "Can't add above " + FormatCoordinates(_x, _y) + ".");

        //same of the other 2 elements
        if (surRight != null)
            if (surRight.GetLeft() != null)
                ThrowSpaceTaken(_x, _y + 1, "Can't add above " + FormatCoordinates(_x, _y) + ".");

        if (surLeft != null)
            if (surLeft.GetRight() != null)
                ThrowSpaceTaken(_x, _y + 1, "Can't add above " + FormatCoordinates(_x, _y) + ".");

        //We got this far, this means the space really isn't taken, so let's fill it.
        Element<T> newElement = new Element<T>(_element, _x, _y + 1);
        rootElement.SetAbove(newElement);
        newElement.SetBelow(rootElement);
        //And we also got to mind the other 3 potential neighbours of our new element
        if (surAbove != null)
        {
            surAbove.SetBelow(newElement);
            newElement.SetAbove(surAbove);
        }
        if (surRight != null)
        {
            surRight.SetLeft(newElement);
            newElement.SetRight(surRight);
        }
        if (surLeft != null)
        {
            surLeft.SetRight(newElement);
            newElement.SetLeft(surLeft);
        }
            
    }

    public void AddBelow(int _x, int _y, T _element)
    {
        Element<T> rootElement = GetElementAt(_x, _y, true); //if the element at the given index doesn't exist, the method will stop here

        if (rootElement == null)
            ThrowElementNotFoundAtIndex(_x, _y);

        if (rootElement.GetBelow() != null)
            ThrowSpaceTaken(_x, _y - 1, "Can't add below " + FormatCoordinates(_x, _y) + ".");

        //check surroundings next
        //there might already be another element here.
        //these 3 elements are the other ones that potentially surround the nem element
        Element<T> surBelow = GetElementAt(_x, _y - 2, false);
        Element<T> surRight = GetElementAt(_x + 1, _y - 1, false);
        Element<T> surLeft = GetElementAt(_x - 1, _y - 1, false);

        //check if the element above my new element already exists
        //and if so, whether it has a below element
        //if both is true, stop here
        if (surBelow != null)
            if (surBelow.GetAbove() != null)
                ThrowSpaceTaken(_x, _y - 1, "Can't add below " + FormatCoordinates(_x, _y) + ".");

        //same of the other 2 elements
        if (surRight != null)
            if (surRight.GetLeft() != null)
                ThrowSpaceTaken(_x, _y - 1, "Can't add below " + FormatCoordinates(_x, _y) + ".");

        if (surLeft != null)
            if (surLeft.GetRight() != null)
                ThrowSpaceTaken(_x, _y - 1, "Can't add below " + FormatCoordinates(_x, _y) + ".");

        //We got this far, this means the space really isn't taken, so let's fill it.
        Element<T> newElement = new Element<T>(_element, _x, _y - 1);
        rootElement.SetBelow(newElement);
        newElement.SetAbove(rootElement);
        //And we also got to mind the other 3 potential neighbours of our new element
        if (surBelow != null)
        {
            surBelow.SetAbove(newElement);
            newElement.SetBelow(surBelow);
        }
        if (surRight != null)
        {
            surRight.SetLeft(newElement);
            newElement.SetRight(surRight);
        }
        if (surLeft != null)
        {
            surLeft.SetRight(newElement);
            newElement.SetLeft(surLeft);
        }           
    }

    public void AddRight(int _x, int _y, T _element)
    {
        Element<T> rootElement = GetElementAt(_x, _y, true); //if the element at the given index doesn't exist, the method will stop here

        if(rootElement == null)
            ThrowElementNotFoundAtIndex(_x, _y);

        if (rootElement.GetRight() != null)
            ThrowSpaceTaken(_x + 1, _y, "Can't add right to " + FormatCoordinates(_x, _y) + ".");

        //check surroundings next
        //there might already be another element here.
        //these 3 elements are the other ones that potentially surround the nem element
        Element<T> surBelow = GetElementAt(_x + 1, _y - 1, false);
        Element<T> surRight = GetElementAt(_x + 2, _y, false);
        Element<T> surAbove = GetElementAt(_x + 1, _y + 1, false);

        //check if the element above my new element already exists
        //and if so, whether it has a below element
        //if both is true, stop here
        if (surBelow != null)
            if (surBelow.GetAbove() != null)
                ThrowSpaceTaken(_x + 1, _y, "Can't add right to " + FormatCoordinates(_x, _y) + ".");

        //same of the other 2 elements
        if (surRight != null)
            if (surRight.GetLeft() != null)
                ThrowSpaceTaken(_x + 1, _y, "Can't add right to " + FormatCoordinates(_x, _y) + ".");

        if (surAbove != null)
            if (surAbove.GetBelow() != null)
                ThrowSpaceTaken(_x + 1, _y, "Can't add right to " + FormatCoordinates(_x, _y) + ".");

        //We got this far, this means the space really isn't taken, so let's fill it.
        Element<T> newElement = new Element<T>(_element, _x + 1, _y);
        rootElement.SetRight(newElement);
        newElement.SetLeft(rootElement);
        //And we also got to mind the other 3 potential neighbours of our new element
        if (surBelow != null)
        {
            surBelow.SetAbove(newElement);
            newElement.SetBelow(surBelow);
        }
        if (surRight != null)
        {
            surRight.SetLeft(newElement);
            newElement.SetRight(surRight);
        }
        if (surAbove != null)
        {
            surAbove.SetBelow(newElement);
            newElement.SetAbove(surAbove);
        }
    }

    public void AddLeft(int _x, int _y, T _element)
    {
        Element<T> rootElement = GetElementAt(_x, _y, true); //if the element at the given index doesn't exist, the method will stop here

        if (rootElement == null)
            ThrowElementNotFoundAtIndex(_x, _y);

        if (rootElement.GetLeft() != null)
            ThrowSpaceTaken(_x - 1, _y, "1 Can't add left to " + FormatCoordinates(_x, _y) + ".");

        //check surroundings next
        //there might already be another element here.
        //these 3 elements are the other ones that potentially surround the nem element
        Element<T> surBelow = GetElementAt(_x - 1, _y - 1, false);
        Element<T> surAbove = GetElementAt(_x - 1, _y + 1, false);
        Element<T> surLeft = GetElementAt(_x - 2, _y, false);

        //check if the element above my new element already exists
        //and if so, whether it has a below element
        //if both is true, stop here
        if (surBelow != null)
            if (surBelow.GetAbove() != null)
                ThrowSpaceTaken(_x - 1, _y, "2 Can't add left to " + FormatCoordinates(_x, _y) + ".");

        //same of the other 2 elements
        if (surAbove != null)
            if (surAbove.GetBelow() != null)
                ThrowSpaceTaken(_x - 1, _y, "3 Can't add left to " + FormatCoordinates(_x, _y) + ".");

        if (surLeft != null)
            if (surLeft.GetRight() != null)
                ThrowSpaceTaken(_x - 1, _y, "4 Can't add left to " + FormatCoordinates(_x, _y) + ".");

        //We got this far, this means the space really isn't taken, so let's fill it.
        Element<T> newElement = new Element<T>(_element, _x - 1, _y);
        rootElement.SetLeft(newElement);
        newElement.SetRight(rootElement);
        //And we also got to mind the other 3 potential neighbours of our new element
        if (surBelow != null)
        {
            surBelow.SetAbove(newElement);
            newElement.SetBelow(surBelow);
        }
        if (surAbove != null)
        {
            surAbove.SetBelow(newElement);
            newElement.SetAbove(surBelow);
        }
        if (surLeft != null)
        {
            surLeft.SetRight(newElement);
            newElement.SetLeft(surLeft);
        }
    }


    public void Remove(int _x, int _y)
    {
        Element<T> element = GetElementAt(_x, _y, false);

        if (element == null)
            return;

        Element<T> t = element.GetAbove();
        if (t != null)
            t.SetBelow(null);

        t = element.GetBelow();
        if (t != null)
            t.SetAbove(null);

        t = element.GetRight();
        if (t != null)
            t.SetLeft(null);

        t = element.GetLeft();
        if (t != null)
            t.SetRight(null);

    }


    public System.Tuple<int, int> GetIndex(T _element)
    {
        ArrayList queue = new ArrayList();

        Element<T> currentElement = centerElement;
        if (centerElement == null)
            return null;

        queue.Add(currentElement);

        Element<T> t;

        while (queue.Count != 0)
        {
            //get current element and remove it from the queue
            currentElement = (Element<T>)queue[0];
            queue.Remove(currentElement);
            //set its marker
            currentElement.SetMarker(true);

            if (_element.Equals(currentElement.GetValue()))
            {
                SetAllMarkers(false);
                return currentElement.GetCoordinate();
            }             

            //get all neghbouring elements and add them to the queue
            t = currentElement.GetAbove();
            if (t != null && !t.GetMarker())
                queue.Add(t);
            t = currentElement.GetBelow();
            if (t != null && !t.GetMarker())
                queue.Add(t);
            t = currentElement.GetRight();
            if (t != null && !t.GetMarker())
                queue.Add(t);
            t = currentElement.GetLeft();
            if (t != null && !t.GetMarker())
                queue.Add(t);
        }

        SetAllMarkers(false);
        return null;
    }

    private void SetAllMarkers(bool _value)
    {
        ArrayList queue = new ArrayList();

        Element<T> currentElement = centerElement;
        if (centerElement == null)
            return;

        queue.Add(currentElement);

        Element<T> t;
        while (queue.Count != 0)
        {
            //get current element and remove it from the queue
            currentElement = (Element<T>) queue[0];
            queue.Remove(currentElement);
            //set its marker
            currentElement.SetMarker(_value);
            //get all neghbouring elements and add them to the queue

            t = currentElement.GetAbove();
            if (t != null && t.GetMarker() != _value)
                queue.Add(t);
            t = currentElement.GetBelow();
            if (t != null && t.GetMarker() != _value)
                queue.Add(t);
            t = currentElement.GetRight();
            if (t != null && t.GetMarker() != _value)
                queue.Add(t);
            t = currentElement.GetLeft();
            if (t != null && t.GetMarker() != _value)
                queue.Add(t);
        }
    }

    /*
     * Goes to the defined location and goes down by _height and right by _width.
     * The resulting set will be returned as an array.
     * 
     * If a certain spot isn't filled in the mesh this part will be returned as null
     */
    public T[,] Get2DArray(int _x, int _y, int _width, int _height)
    {
        T[,] ret = new T[_width, _height];

        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                Element<T> t = GetElementAt(_x + i, _y + j, false);
                if(t != null)
                    ret[i, j] = t.GetValue();
            }
        }

        return ret;
    }


    private bool CheckCenter(bool _throwException)
    {
        if (centerElement == null)
        {
            if(_throwException)
                ThrowCenterNotFound();

            return false;
        }         

        return true;
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //++++++++++++++++++++++++++++++++ HELPER +++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    private string FormatCoordinates(int _x, int _y)
    {
        return "(" + _x + "|" + _y + ")";
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++ EXCEPTIONS ++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    private void ThrowSpaceTaken(int _x, int _y, string _message)
    {
        throw new System.Exception("Space " + FormatCoordinates(_x, _y) + " is already taken. " + _message);
    }

    private void ThrowElementNotFoundAtIndex(int _x, int _y)
    {
        throw new System.Exception("Element " + FormatCoordinates(_x, _y) + " does not exist in the MeshList.");
    }

    private void ThrowCenterNotFound()
    {
        throw new System.Exception("The MeshList doesn't have a center yet.");
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //++++++++++++++++++++++++++++++ ELEMENT CLASS ++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    private class Element<P>
    {
        private Element<P> above;
        private Element<P> below;
        private Element<P> right;
        private Element<P> left;

        private bool marker;
        private P value;
        private int x;
        private int y;

        public Element(P _value, int _x, int _y)
        {
            value = _value;
            x = _x;
            y = _y;
        }

        public void SetMarker(bool _value)
        {
            marker = _value;
        }

        public bool GetMarker()
        {
            return marker;
        }

        public void SetValue(P _value)
        {
            value = _value;
        }

        public P GetValue()
        {
            return value;
        }

        public Element<P> GetAbove()
        {
            return above;
        }

        /*
         * Sets the neighbour element above.
         * If there already is one it will be replaced.
         */
        public void SetAbove(Element<P> _element)
        {
            above = _element;
        }

        public Element<P> GetBelow()
        {
            return below;
        }

        /*
         * Sets the neighbour element below.
         * If there already is one it will be replaced.
         */
        public void SetBelow(Element<P> _element)
        {
            below = _element;
        }

        public Element<P> GetRight()
        {
            return right;
        }

        /*
         * Sets the neighbour element right.
         * If there already is one it will be replaced.
         */
        public void SetRight(Element<P> _element)
        {
            right = _element;
        }

        public Element<P> GetLeft()
        {
            return left;
        }

        /*
         * Sets the neighbour element above.
         * If there already is one it will be replaced.
         */
        public void SetLeft(Element<P> _element)
        {
            left = _element;
        }

        public System.Tuple<int, int> GetCoordinate()
        {
            return new System.Tuple<int, int>(x, y);
        }
    }
}
