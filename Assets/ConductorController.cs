using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.CoordinateSystem;

namespace Mediapipe.Unity
{
public class ConductorController : MonoBehaviour
{
    float prevTime;
    Vector2 cur, prev, pprev;
    float prevDistance, curDistance;
    bool isBeat;
    bool isAsc;
    bool isDirty;
    
    private enum BeatState {Undefined, First, First_One, First_Two, Second, Third, Third_One, Third_Two, Forth, Forth_One};
    BeatState curState, prevState;
    Vector2 central;
    

    AudioSource _click;

    void Start ()
    {
        _click = GetComponent<AudioSource>();
        isBeat = false;
        isAsc = false;
        isDirty = false;

        curState = BeatState.Undefined;
        prevState = BeatState.Undefined;
    }

    void Update()
    {
        Calc();
        
        if (OnBeat()) {
            Click();
        }
    }

    private void Click ()
    {
        if (_click.isPlaying)
            _click.Stop();
        _click.Play();
    }

    private void Calc ()
    {
        var now = Time.time;
        if (!isDirty) return;
        var vec_x = cur.x - prev.x;
        var vec_y = cur.y - prev.y;
        var err = 0.05f;

        Debug.Log(curState.ToString() + ", " + prevState.ToString());

        if (Math.Abs(vec_y) > err)
        {
            if (prevState == BeatState.Forth && curState == BeatState.Forth && vec_y > 0)
            {
                central = cur;
                curState = BeatState.Forth_One;    
            }
            else if (prevState == BeatState.First && curState == BeatState.First && (vec_y < -err || vec_x > err))
            {
                curState = BeatState.First_One;
            }
            else if (prevState == BeatState.First_One && curState == BeatState.First_One && vec_y > 0)
            {
                curState = BeatState.First_Two;
            }
            else if (prevState == BeatState.First_Two && curState == BeatState.First_Two && vec_y < 0 && vec_x > 0)
            {
                curState = BeatState.Second;
            }
            else if (prevState == BeatState.Second && curState == BeatState.Second && vec_y < 0 && cur.x > central.x)
            {
                curState = BeatState.Third;
            }
            else if (prevState == BeatState.Third && curState == BeatState.Third && vec_y < 0)
            {
                curState = BeatState.Third_One;
            }
            else if (prevState == BeatState.Third_One && curState == BeatState.Third_One && vec_y > 0)
            {
                curState = BeatState.Third_Two;
            }
            else if (prevState == BeatState.Third_Two && curState == BeatState.Third_Two && vec_y < 0)
            {
                curState = BeatState.Forth;
            }
            else if (curState == BeatState.Undefined)
            {
                curState = BeatState.Forth;
            }
            prev = cur;
            isDirty = false;
        }
        
        if (Math.Abs(vec_x) > err)
        {
            
            if (prevState == BeatState.Forth_One && curState == BeatState.Forth_One && vec_x > 0)
            {
                curState = BeatState.First;
            }

            prev = cur;
            isDirty = false;
        }
    }

    private bool OnBeat ()
    {
        if (curState != prevState)
        {
            prevState = curState;
            if (curState == BeatState.First || curState == BeatState.Second || curState == BeatState.Third || curState == BeatState.Forth)
                return true;
            return false;
        }

        return false;
    }

    public void Process (List<NormalizedLandmarkList> landmarkLists)
    {
        if (landmarkLists == null || landmarkLists.Count <= 0) return;

        var firstHand = landmarkLists[0];
        var landmark = firstHand.Landmark[8];
        cur = new Vector2(landmark.X, landmark.Y);

        // if (Vector2.Distance(cur, last1) < 0.05) return;
        
        curDistance = Vector3.Distance(cur, prev);

        isDirty = true;
    }
}
}