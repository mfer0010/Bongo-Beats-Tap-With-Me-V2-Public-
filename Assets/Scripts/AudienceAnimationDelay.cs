using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This Script creates a random delay for aniations to start so the crowd seems less robotic
public class AudienceAnimationDelay : MonoBehaviour
{
    private Animation animation_ref;

    void Awake()
    {
        animation_ref = GetComponent<Animation>();
        if (!animation_ref)
        {
            Debug.LogWarning("No animation found in " + gameObject.name);
        }
        else
        {
            animation_ref.playAutomatically = false;
        }
    }

    private void Start()
    {
        float delay = Random.Range(0, 2);

        //Debug.Log(gameObject.name + " " + delay);

        StartCoroutine(StartAnimation(delay));
    }

    IEnumerator StartAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);

        animation_ref.Play();
    }
}
