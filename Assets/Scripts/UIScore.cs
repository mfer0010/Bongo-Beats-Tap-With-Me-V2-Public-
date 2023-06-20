using System.Collections;
using TMPro;
using UnityEngine;

public class UIScore : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    // 0, ... , 9
    [SerializeField] private AudioClip[] digitsAudio;
    // 11, ... , 19
    [SerializeField] private AudioClip[] teensAudio;
    // 10, 20, 30, ... , 90
    [SerializeField] private AudioClip[] tensAudio;
    // 1000, 100
    [SerializeField] private AudioClip thousand, hundred;
    // en
    [SerializeField] private AudioClip en;

    private AudioSource audioSource;

    private void Awake()
    {
        if (!scoreText)
        {
            Debug.LogWarning("Please assign scoreText Variable in " + gameObject.name);
        }

        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            Debug.LogError("Please ensure " + gameObject.name + " has an AudioSource component attached");
        }

        if (digitsAudio.Length != 10)
        {
            Debug.LogError("Please ensure all 10 digits (0-9) are present in the digits audio clip array in " + gameObject.name);
        }

        if (teensAudio.Length != 9)
        {
            Debug.LogError("Please ensure teens audio clip array contains the numbers 11 to 19 in " + gameObject.name);
        }

        if (tensAudio.Length != 9)
        {
            Debug.LogError("Please ensure tens audio clip array contains the numbers 10, 20, ... , 90 in " + gameObject.name);
        }
    }

    private void FixedUpdate()
    {
        scoreText.text = ScoreCounter.GetScore().ToString("0000");
    }

    private void Update()
    {
        if (Input.GetButtonDown("Score"))
        {
            //Debug.Log("Dictating Score");
            DictateScore();
        }
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }

    // When called, will dictate the score
    private void DictateScore()
    {
        char[] digits = new char[4];
        digits = scoreText.text.ToCharArray();
        StartCoroutine(Dictate_V2(digits));
    }

    

    // Dictates score as a whole number rather than as individual digits
    public IEnumerator Dictate_V2(char[] digits)
    {
        if((digits[0] - '0') != 0)
        {
            if ((digits[1] - '0') == 0)
            {
                if ((digits[0] - '0') != 1)
                {
                    audioSource.clip = digitsAudio[digits[0] - '0'];
                    audioSource.Play();
                    yield return new WaitForSecondsRealtime(audioSource.clip.length);
                }
                audioSource.clip = thousand;
                audioSource.Play();
                yield return new WaitForSecondsRealtime(audioSource.clip.length);
            }
            else
            {
                // Read first 2 digits as numbers
                StartCoroutine(ReadDigitsAsWholeNumber(digits[0] - '0', digits[1] - '0'));
            }
        }
        else
        {
            if ((digits[1] - '0') != 0)
            {
                if ((digits[1] - '0') != 1)
                {
                    audioSource.clip = digitsAudio[digits[1] - '0'];
                    audioSource.Play();
                    yield return new WaitForSecondsRealtime(audioSource.clip.length);
                }
                audioSource.clip = hundred;
                audioSource.Play();
                yield return new WaitForSecondsRealtime(audioSource.clip.length);
            }
            else if (digits[2] - '0' == 0 && digits[3] - '0' == 0)
            {
                audioSource.clip = digitsAudio[0];
                audioSource.Play();
                yield break;
            }
        }
        // Read last 2 digits as numbers
        yield return StartCoroutine(ReadDigitsAsWholeNumber(digits[2] - '0', digits[3] - '0'));
    }

    // Reads 2 digits as numbers in Dutch (number should be in the order digit1digit2)
    // Example for 45 digit1 = 4, digit2 = 5
    private IEnumerator ReadDigitsAsWholeNumber(int digit1, int digit2)
    {
        // Do nothing if both digits are 0
        if (digit1 == 0 && digit2 == 0)
            yield break;

        if (digit1 == 0)
        {
            audioSource.clip = digitsAudio[digit2];
            audioSource.Play();
            yield break;            
        }
        else if (digit1 == 1)
        {
            if (digit2 == 0)
            {
                // play ten only
                audioSource.clip = tensAudio[0];
                audioSource.Play();
                yield break;
            }
            else
            {
                // play teens
                audioSource.clip = teensAudio[digit2 - 1];
                audioSource.Play();
                yield break;
            }
        }
        else
        {
            if (digit2 == 0)
            {
                // play tens only
                audioSource.clip = tensAudio[digit1 - 1];
                audioSource.Play();
                yield break;
            }
            else
            {
                // Play whole number
                audioSource.clip = digitsAudio[digit2];
                audioSource.Play();
                yield return new WaitForSecondsRealtime(audioSource.clip.length);
                audioSource.clip = en;
                audioSource.Play();
                yield return new WaitForSecondsRealtime(audioSource.clip.length);
                audioSource.clip = tensAudio[digit1 - 1];
                audioSource.Play();
                yield break;
            }
        }
    }
}
