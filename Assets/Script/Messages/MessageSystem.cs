using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;

// After X seconds, a message appears.
// When that message is closed, a timer starts again and opens the next message after X more seconds. Repeat.
public class MessageSystem : MonoBehaviour
{
#region  Singleton
    public static MessageSystem Instance { get; private set; }

    private void Awake() 
    { 
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }
#endregion

    [Header("Design Variables")]
    [SerializeField] private int secondsBetweenMessages = 5;
    [SerializeField] private List<MessageData> messages;
    
    private MessageData curMessage;
    [SerializeField] private int curMessageIndex = 0;
    
    // Currently only one message at a time
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI  messageText;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private GameObject notificationPopup;
    [SerializeField] private Image profilePictureImage;
    [SerializeField] private TextMeshProUGUI characterName;


    [SerializeField] private float slideTime = 0.5f;
    
    public delegate void OnMessagePopUp(bool badMessage);
    public OnMessagePopUp onMessageOpen;
    
    public delegate void OnMessageClose();
    public OnMessageClose onMessageClose;
    
    public delegate void OnMessageCloseDelayPassed(MessageData messageData);
    public OnMessageCloseDelayPassed onMessageCloseDelayPassed;
    
    //ghostboy to remove during refactor
    [SerializeField] private GameObject ghostBoy;
    public bool isEnd = false;
    [SerializeField] private GameObject fog;
    
    private void Start()
    {
        // messagePanel.SetActive(false);
        onMessageClose += StartNextPopUpTimer;
        StartCoroutine(OpenPopUpAfterSeconds());
    }

    private void StartNextPopUpTimer()
    {
        if (curMessageIndex >= messages.Count) {
            return;
        }
        StartCoroutine(OpenPopUpAfterSeconds());
    }

    private IEnumerator OpenPopUpAfterSeconds()
    {
        Invoke(nameof(InitializeNextPopUp), 1f);
        yield return new WaitForSecondsRealtime(secondsBetweenMessages);
        
        float delay = 0;
        MessageData messageCheck = curMessage;

        // Notification popup
        notificationPopup.SetActive(true);
        while (delay < 1 )
        {
            if(messageCheck != curMessage)
                yield break;
            delay += Time.deltaTime;
            yield return null;
        }
        notificationPopup.SetActive(false);


        OpenNextPopUp();
        messageText.text = curMessage.messageText;
        // onMessageCloseDelayPassed.Invoke(curMessage);
    }

    private void OpenNextPopUp()
    {
        // messagePanel.SetActive(true);
        StartCoroutine(SlidePhone(-900f, 0f));
        onMessageOpen.Invoke(curMessage.ticksAnxiety);
    }
    
    private void InitializeNextPopUp()
    {
        
        if (messages.Count > curMessageIndex)
        {
            curMessage = messages[curMessageIndex];
            curMessageIndex++;
            messageText.text = curMessage.cutoffText;

            // Debug.Log(curMessage.profilePicture);
            profilePictureImage.sprite = curMessage.profilePicture;
            characterName.text = curMessage.characterName;
        }
    }
    
    private void HidePopUp()
    {
        // messagePanel.SetActive(false);
        StartCoroutine(SlidePhone(0f, -900f));
        onMessageClose.Invoke();

    }

    public static void ClosePopUp()
    {
        Instance.HidePopUp();
    }


    public IEnumerator SlidePhone(float start, float end) {

        float timer = 0f;

        while (timer <= slideTime) {

            float yPos = Mathf.SmoothStep(start, end, timer / slideTime);
            messagePanel.GetComponent<RectTransform>().localPosition = new Vector3(0f, yPos, 0f);

            timer += Time.deltaTime;
            yield return null;
        }
        messagePanel.GetComponent<RectTransform>().localPosition = new Vector3(0f, end, 0f);
    }

    //hack method for last minute, set ghostboy to active
    private void Update()
    {
        if (messages.Count == curMessageIndex)
        {
            isEnd = true;
            Invoke("SpawnGhostBoy", 12f);
            curMessageIndex += 1;
        } 
    }

    private void SpawnGhostBoy()
    {
        if (isEnd && !ghostBoy.activeSelf)
        {
            ghostBoy.SetActive(true);
            fog.SetActive(false);
        }
    }
}