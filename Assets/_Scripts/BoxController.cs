using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CellState
{
    Empty,
    MarkedX,
    Dog
}

public class BoxController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject dogImage;
    [SerializeField] private GameObject xImage;

    [Header("Click Settings")]
    [SerializeField] private float clickDelay = 0.25f;

    public int Row { get; private set; }
    public int Col { get; private set; }
    public int RegionId { get; private set; }
    public CellState CurrentState { get; private set; } = CellState.Empty;

    private float _lastClickTime = -1f;

    public void Initialize(int row, int col, int regionId, Color color)
    {
        Row = row;
        Col = col;
        RegionId = regionId;

        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }

        // Hide markers initially
        if (dogImage != null) dogImage.SetActive(false);
        if (xImage != null) xImage.SetActive(false);
        CurrentState = CellState.Empty;

        // Pop-in Animation
        this.transform.localScale = Vector3.zero;
        this.transform.localRotation = Quaternion.Euler(0, 0, 40);

        GameManager.Instance.boxes[Row, Col] = this;
        GameManager.Instance.EventManager.popUpButtonsEvent += PopOut;
    }

    /// <summary>
    /// Intercepts UI Pointer Clicks directly from Unity's EventSystem.
    /// </summary>
    public void OnPointerClick()
    {
        float timeSinceLastClick = Time.time - _lastClickTime;

        if (timeSinceLastClick <= clickDelay)
        {
            // Cancel any pending single click action and trigger Double Click
            CancelInvoke(nameof(OnSingleClick));
            OnDoubleClick();
            _lastClickTime = -1f; // Reset timer to prevent triple-click artifacts
        }
        else
        {
            _lastClickTime = Time.time;
            // Delay the single click slightly to see if a second tap occurs
            Invoke(nameof(OnSingleClick), clickDelay);
        }
    }

    private void OnSingleClick()
    {
        // Toggle the X Marker on single click
        if (CurrentState == CellState.Empty)
        {
            CurrentState = CellState.MarkedX;
            if (xImage != null) xImage.SetActive(true);
            if (dogImage != null) dogImage.SetActive(false);
        }
        else if (CurrentState == CellState.MarkedX)
        {
            ClearCell();
        }
    }

    private void OnDoubleClick()
    {
        // Cancel single click invoke just in case
        CancelInvoke(nameof(OnSingleClick));

        // Ask the rule validator if placing a Dog here is valid according to Meowdoku rules

        if(CurrentState == CellState.Dog) 
        {
            // If the cell already has a dog, we can clear it on double click
            GameManager.Instance.RecallDog();
            ClearCell();
            return;
        }

        bool isValid = MeowdokuRuleValidator.IsValidMove(Row, Col, RegionId);

        if (isValid)
        {
            CurrentState = CellState.Dog;
            if (dogImage != null)
            {
                dogImage.SetActive(true);
                // Optional: Add a cute bounce effect when placing the dog sprite
                dogImage.transform.localScale = Vector3.zero;
                dogImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }
            if (xImage != null) xImage.SetActive(false);
            GameManager.Instance.OnValidMove();
            // Inform the game manager that a valid move was made (to check win condition)
        }
        else
        {
            // Invalid move! Deduct a heart and play a shake animation
            TriggerErrorShake();
        }
    }

    void PopOut()
    {
        this.transform.DOScale(1, 1).SetEase(Ease.OutBack);
        this.transform.DORotate(Vector3.zero, 1).SetEase(Ease.OutBack);
    }

    public void ClearCell()
    {
        CurrentState = CellState.Empty;
        if (xImage != null) xImage.SetActive(false);
        if (dogImage != null) dogImage.SetActive(false);
    }

    private void OnDestroy()
    {
        GameManager.Instance.EventManager.popUpButtonsEvent -= PopOut;
    }

    private void TriggerErrorShake()
    {
        // Shake the button visually to indicate a rule violation
        this.transform.DOShakePosition(0.4f, strength: 10f, vibrato: 20);
    }
}