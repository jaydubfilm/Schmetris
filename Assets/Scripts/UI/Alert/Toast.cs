using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Toast : Singleton<Toast>
{
    private const float OFFSET = 20f;
    
    public enum Layout
    {
        Start,
        Middle,
        End
    }

    private struct ToastData
    {
        public float time { get; set; }

        public Sprite sprite { get; set; }
        public string Text { get; set; }

        public Layout verticalLayout { get; set; }
        public Layout horizontalLayout { get; set; }
    }
    
    //================================================================================================================//
    [SerializeField, Required] private RectTransform toastArea;
    
    [SerializeField, Required]
    private GameObject toastGameObject;
    [SerializeField, Required]
    private CanvasGroup canvasGroup;
    [SerializeField, Required]
    private Image Image;
    [SerializeField, Required]
    private TMP_Text Text;
    [SerializeField, Required]
    private Slider timeSlider;

    private RectTransform toastTransform;

    public bool showingToast { get; private set; }
    
    private Queue<ToastData> pendingToasts;
    
    //================================================================================================================//
    
    // Start is called before the first frame update
    private void Start()
    {
        toastTransform = toastGameObject.transform as RectTransform;
        
        pendingToasts = new Queue<ToastData>();

        toastGameObject.SetActive(false);
    }
    
    //================================================================================================================//

    public static void SetToastArea(in RectTransform newArea)
    {
        var toastArea = Instance.toastArea;
        
        toastArea.anchorMin = newArea.anchorMin;
        toastArea.anchorMax = newArea.anchorMax;

        toastArea.sizeDelta = newArea.sizeDelta;
        toastArea.anchoredPosition = newArea.anchoredPosition;
    }

    public static void AddToast(string text, Sprite sprite = null, float time = 2f, Layout verticalLayout = Layout.End,
        Layout horizontalLayout = Layout.End)
    {
        Instance?.Add(new ToastData
        {
            Text = text,
            sprite = sprite,
            time = time,
            verticalLayout = verticalLayout,
            horizontalLayout = horizontalLayout
        });
    }
    
    //================================================================================================================//

    private void Add(ToastData toast)
    {
        pendingToasts.Enqueue(toast);

        if (showingToast)
            return;

        StartCoroutine(ShowToastsCoroutine());
    }

    //================================================================================================================//

    private const bool USE_SLIDER = false;
    private IEnumerator ShowToastsCoroutine()
    {
        if(showingToast)
            yield break;
        
        showingToast = true;

        while (pendingToasts.Count > 0)
        {
            var toast = pendingToasts.Dequeue();
            toastGameObject.SetActive(true);
            timeSlider.gameObject.SetActive(USE_SLIDER);
            
            canvasGroup.alpha = 1f;
            Image.sprite = toast.sprite;
            Text.text = toast.Text;
            SetPosition(toastTransform, toast);
            
            Image.gameObject.SetActive(toast.sprite != null);
            
            var t = 0f;

            while (t < toast.time)
            {
                t += Time.deltaTime;
                
                if(USE_SLIDER)
                    timeSlider.value = 1f - t / toast.time;
                yield return null;
            }
            
            t = 0f;
            
            while (t < 1f)
            {
                t += Time.deltaTime * 3f;
                canvasGroup.alpha = 1f - t;
                yield return null;
            }
            
            toastGameObject.SetActive(false);
            
            yield return new WaitForSeconds(0.2f);
        }

        showingToast = false;
    }

    private static void SetPosition(RectTransform transform, ToastData toastData)
    {
        var targetAnchor = Vector2.zero;
        var targetPosition = Vector2.zero;
        
        switch (toastData.horizontalLayout)
        {
            case Layout.Start:
                targetAnchor.x = 0f;
                targetPosition.x = OFFSET;
                break;
            case Layout.Middle:
                targetAnchor.x = 0.5f;
                break;
            case Layout.End:
                targetAnchor.x = 1f;
                targetPosition.x = -OFFSET;
                break;
        }
        
        switch (toastData.verticalLayout)
        {
            case Layout.Start:
                targetAnchor.y = 1f;
                targetPosition.y = -OFFSET;
                break;
            case Layout.Middle:
                targetAnchor.y = 0.5f;
                break;
            case Layout.End:
                targetAnchor.y = 0f;
                targetPosition.y = OFFSET;
                break;
        }

        transform.anchorMin = transform.anchorMax = targetAnchor;
        transform.pivot = targetAnchor;
        transform.anchoredPosition = targetPosition;
    }

    //================================================================================================================//
    
    #if UNITY_EDITOR

    [Button("Create Test Toast"), DisableInEditorMode]
    private void TestToast()
    {
        var layouts = new[]
        {
            Layout.Start,
            Layout.End
        };
        
        AddToast($"Test Toast {Random.Range(0,100)}", null,
            Random.Range(1f,2f),
            layouts[Random.Range(0, layouts.Length)],
            layouts[Random.Range(0, layouts.Length)]
            );
    }
    
    #endif

}
