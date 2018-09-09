using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Frame : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{

    [HideInInspector]
    public Vector3 startPosition;
    [HideInInspector]
    public Animator animator;


    private readonly int HashActive = Animator.StringToHash("Active");
    private readonly int HashHoverOn = Animator.StringToHash("HoverOn");
    private readonly int HashDisabled = Animator.StringToHash("Disabled");
    private readonly int HashCharacterOn = Animator.StringToHash("CharacterOn");

    private RectTransform rectTransfrom;
    private PlayerPlatformerController m_Player;
    private Collider2D[] m_Colliders;
    private SortingGroup m_ObjectsSortingGroup;

    public bool Disabled { get; private set; }
    public bool IsBeingDragged { get; private set; }

    private void Awake()
    {
        startPosition = transform.position;
        animator = GetComponent<Animator>();
        rectTransfrom = GetComponent<RectTransform>();

        m_Colliders = GetComponentsInChildren<Collider2D>();

        m_ObjectsSortingGroup = GetComponentInChildren<SortingGroup>();

        m_Player = FindObjectOfType<PlayerPlatformerController>();


    }

    private void Start()
    {
        if (FrameContainsPosition(Camera.main.WorldToScreenPoint(m_Player.transform.position)))
        {
            SetCharacterOn(true);
        }

    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        if (Disabled) return;

        if(FrameContainsPosition(Camera.main.WorldToScreenPoint(m_Player.transform.position)))
        {
            IsBeingDragged = false;
            return;
        }

        IsBeingDragged = true;

        startPosition = transform.position;

        EnableDragEssentials();
    }


    public void OnDrag(PointerEventData eventData)
    {

        if (!IsBeingDragged) return;

        Vector3 touchedPosition = GetTouchedPosition();

        Vector3 position = Camera.main.ScreenToWorldPoint(touchedPosition);
        transform.position = new Vector3(position.x, position.y, transform.position.z);

        if (FrameCollection.Instance.PreviousBeingHoverOnFrame != null)
        {
            FrameCollection.Instance.PreviousBeingHoverOnFrame.animator.SetBool(HashHoverOn, false);
        }

        Frame frame;

        if (FrameCollection.Instance.FrameContainsPosition(this, touchedPosition, out frame) && !frame.Disabled &&
            !frame.FrameContainsPosition(Camera.main.WorldToScreenPoint(m_Player.transform.position)))
        {
            frame.animator.SetBool(HashHoverOn, true);
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {

        if (!IsBeingDragged) return;

        if (FrameCollection.Instance.PreviousBeingHoverOnFrame != null)
        {
            FrameCollection.Instance.PreviousBeingHoverOnFrame.animator.SetBool(HashHoverOn, false);
        }

        Frame frame;
        if (FrameCollection.Instance.FrameContainsPosition(this, GetTouchedPosition(), out frame) && !frame.Disabled &&
            !frame.FrameContainsPosition(Camera.main.WorldToScreenPoint( m_Player.transform.position)))
        {
            FrameCollection.Instance.SwitchBetween(this, frame);
        }
        else
        {
            transform.position = startPosition;
        }

        DisableDragEssentials();

        IsBeingDragged = false;

    }

    private void EnableDragEssentials()
    {
        animator.SetBool(HashActive, true);

        m_ObjectsSortingGroup.sortingOrder = 15;

        //Avoid player jump on being-dragged colliders
        SetCollidersActive(false);

        TimeManager.Instance.SlowdownTime(0.05f, -1f);
    }

    private void DisableDragEssentials()
    {
        animator.SetBool(HashActive, false);

        m_ObjectsSortingGroup.sortingOrder = 2;

        SetCollidersActive(true);

        TimeManager.Instance.ChangeTimeBackToNormal();
    }

    public bool FrameContainsPosition(Vector3 screenPosition)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransfrom, screenPosition, Camera.main);
    }


    public void SetCollidersActive(bool actived)
    {
        for (int i = 0; i < m_Colliders.Length; i++)
        {
            m_Colliders[i].enabled = actived;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        PlayerPlatformerController alessia = collision.GetComponent<PlayerPlatformerController>();

        if (alessia != null)
        {
            Frame nextFrame = FrameCollection.Instance.GetNextFrame(this);

            if(nextFrame!=null)
            {
                nextFrame.SetCharacterOn(true);

                Vector3 newPosition = new Vector3(nextFrame.transform.position.x - nextFrame.GetComponentInChildren<SpriteRenderer>().bounds.extents.x - alessia.GetComponent<Collider2D>().bounds.extents.x / 2,
                     nextFrame.transform.position.y + (alessia.transform.position.y - transform.position.y), 0);

                alessia.transform.position = newPosition;

            }

            this.Disable();

        }

    }

    public void Disable()
    {
        animator.SetBool(HashCharacterOn, false);
        animator.SetTrigger(HashDisabled);
        m_ObjectsSortingGroup.sortingOrder = 15;
        IsBeingDragged = false;
        Disabled = true;

    }

    public void SetCharacterOn(bool characterOn)
    {
        animator.SetBool(HashCharacterOn, characterOn);

        if (characterOn && IsBeingDragged)
        {
            transform.position = startPosition;

            if (FrameCollection.Instance.PreviousBeingHoverOnFrame != null)
            {
                FrameCollection.Instance.PreviousBeingHoverOnFrame.animator.SetBool(HashHoverOn, false);
            }

            DisableDragEssentials();

        }

        IsBeingDragged = !characterOn;
    }

    private Vector3 GetTouchedPosition()
    {

#if UNITY_EDITOR

        return  Input.mousePosition;

#else  
        return Input.GetTouch(0).position;
#endif

    }

}
