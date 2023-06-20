using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure; // Required in C#
using UnityEngine;

public enum Side
{
    Left,
    Right
}

public class DrumStickController : MonoBehaviour
{
    // Left Stick or Right Stick
    public Side side;

    private Animator animator_ref;
    private bool bIsTriggeredLeft, bIsTriggeredRight;

    private bool playerIndexSet = false;
    private PlayerIndex playerIndex;
    private GamePadState state;
    private GamePadState prevState;

    private bool bIsVibrating;
    private bool extendVibration;

    private void Awake()
    {
        animator_ref = GetComponent<Animator>();
        if (!animator_ref)
        {
            Debug.LogError(gameObject.name + " has no animator component");
        }

        bIsTriggeredLeft = false;
        bIsTriggeredRight = false;
        bIsVibrating = false;
        extendVibration = false;
    }

    private void Update()
    {
        UpdateControllerState();

        if (bIsTriggeredLeft && Input.GetAxis("LeftTrigger") != 1)
            bIsTriggeredLeft = false;

        if (bIsTriggeredRight && Input.GetAxis("RightTrigger") != 1)
            bIsTriggeredRight = false;


        CheckInput();        
    }

    private void CheckInput()
    {
        if (side == Side.Left)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || (Input.GetAxis("LeftTrigger") == 1 && !bIsTriggeredLeft))
            {
                bIsTriggeredLeft = true;
                animator_ref.SetTrigger("Hit");
            }
            animator_ref.SetTrigger("Idle");
        }
        if (side == Side.Right)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || (Input.GetAxis("RightTrigger") == 1 && !bIsTriggeredRight))
            {
                bIsTriggeredRight = true;
                animator_ref.SetTrigger("Hit");
            }
            animator_ref.SetTrigger("Idle");
        }
    }

    private void UpdateControllerState()
    {
        // Find a PlayerIndex, for a single player game
        // Will find the first controller that is connected ans use it
        if (!playerIndexSet || !prevState.IsConnected)
        {
            for (int i = 0; i < 4; ++i)
            {
                PlayerIndex testPlayerIndex = (PlayerIndex)i;
                GamePadState testState = GamePad.GetState(testPlayerIndex);
                if (testState.IsConnected)
                {
                    // Debug.Log(string.Format("GamePad found {0}", testPlayerIndex));
                    playerIndex = testPlayerIndex;
                    playerIndexSet = true;
                }
            }
        }

        prevState = state;
        state = GamePad.GetState(playerIndex);
    }

    public void VibrateController()
    {
        if (!bIsVibrating)
        {
            GamePad.SetVibration(playerIndex, Variables.musicianFeedbackVibrationIntensity, Variables.musicianFeedbackVibrationIntensity);
            bIsVibrating = true;

            StartCoroutine(StopVibration());
        }
        else
        {
            extendVibration = true;
        }
    }

    private IEnumerator StopVibration()
    {
        yield return new WaitForSeconds(Variables.musicianFeedbackTimeToStopVibration);

        if (extendVibration)
        {
            StartCoroutine(StopVibration());
        }
        else
        {
            GamePad.SetVibration(playerIndex, 0, 0);
            bIsVibrating = false;
            extendVibration = false;
        }
    }
}
