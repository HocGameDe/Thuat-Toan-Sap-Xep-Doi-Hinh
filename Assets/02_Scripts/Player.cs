using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum MoveType
{
    Circle,
    Square
}
public class Player : MonoBehaviour
{
    public static Player Instance;
    [SerializeField] private GameObject rectangleDraw;
    private List<ISelected> selectedObjects = new List<ISelected>();
    private Vector2 mousePosBeginHold;
    private Vector2 rectangleDrawPosition;
    private Vector2 leftBottomPos;
    private Vector2 rightTopPos;
    private Vector2 sizeScale;
    private RaycastHit2D[] hits;
    [SerializeField] private float spaceBetweenSoldier=1.1f;
    [SerializeField] private int countCircle=6;
    [SerializeField] private bool canRandomPosition;
    [SerializeField] private float spaceRandomPosition = 0.1f;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        SetActiveSelectedArea(false);
        InputManager.Instance.SubEventInput(EventInputCategory.MouseDownLeft, () => SetMouseBeginHold());
        InputManager.Instance.SubEventInput(EventInputCategory.MouseHoldLeft, () => { DrawSelectArea(mousePosBeginHold, InputManager.Instance.mousePoistion); });
        InputManager.Instance.SubEventInput(EventInputCategory.MouseUpLeft, () => { SelectAllSoildierOnAreaSelected(); });
        InputManager.Instance.SubEventInput(EventInputCategory.MouseDownRight, () => { SwitchMoveType(); });
    }
    private void SetMouseBeginHold()
    {
        mousePosBeginHold = InputManager.Instance.mousePoistion;
    }
    public void DrawSelectArea(Vector2 mousePosBegin, Vector2 mousePosEnd)
    {
        SetActiveSelectedArea(true);
        leftBottomPos.x = Math.Min(mousePosBegin.x, mousePosEnd.x);
        leftBottomPos.y = Math.Min(mousePosBegin.y, mousePosEnd.y);
        rightTopPos.x = Math.Max(mousePosBegin.x, mousePosEnd.x);
        rightTopPos.y = Math.Max(mousePosBegin.y, mousePosEnd.y);
        sizeScale = rightTopPos - leftBottomPos;
        rectangleDraw.transform.localScale = sizeScale;
        rectangleDrawPosition.x = leftBottomPos.x + sizeScale.x / 2;
        rectangleDrawPosition.y = leftBottomPos.y + sizeScale.y / 2;
        rectangleDraw.transform.position = rectangleDrawPosition;
    }

    private Vector2 newPosSoldier;
    private Vector2 randomDirectionPos;
    private int countSoldierCurrent;
    private int index;
    private int indexCurrentCircle;
    private int radiusCircle;
    private MoveType moveType = MoveType.Circle;
    public void ArrangeSquadToCircle()
    {
        if (selectedObjects.Count > 0)
        {
            selectedObjects[0].ActionWhenSelected(InputManager.Instance.mousePoistion);
            countSoldierCurrent = 0;
            index = 1;
            while(index<selectedObjects.Count)
            {
                if(index>countSoldierCurrent) countSoldierCurrent += countCircle;
                radiusCircle = countSoldierCurrent / countCircle;
                for (indexCurrentCircle = 0; indexCurrentCircle < countSoldierCurrent; indexCurrentCircle++,index++)
                {
                    if (index == selectedObjects.Count) return;
                    newPosSoldier = Quaternion.Euler(0, 0, 360 / countSoldierCurrent *(indexCurrentCircle+1)) * Vector2.right * radiusCircle* spaceBetweenSoldier;
                    newPosSoldier = InputManager.Instance.mousePoistion + newPosSoldier;
                    if (canRandomPosition) RandomPosition(); else randomDirectionPos = Vector2.zero;
                    selectedObjects[index].ActionWhenSelected(newPosSoldier+ randomDirectionPos);
                }
            }
        }      
    }
    private int width;
    private int height;
    public void ArrangeSquadToSquare()
    {
        if (selectedObjects.Count > 0)
        {
            width = height = (int)Math.Ceiling(Math.Sqrt(selectedObjects.Count));
            index = 0;
            for (int i = 0; i < height; i++)
                for(int j = 0; j < width; j++,index++)
                {
                    if (index == selectedObjects.Count) return;
                    newPosSoldier.x = j* spaceBetweenSoldier;
                    newPosSoldier.y = i* spaceBetweenSoldier;
                    newPosSoldier = InputManager.Instance.mousePoistion - newPosSoldier + Vector2.one*width*0.65f/2;
                    if (canRandomPosition) RandomPosition(); else randomDirectionPos = Vector2.zero;
                    selectedObjects[index].ActionWhenSelected(newPosSoldier+ randomDirectionPos);
                }
        }
    }
    private void RandomPosition()
    {
        randomDirectionPos.x= Random.Range(-spaceRandomPosition, spaceRandomPosition);
        randomDirectionPos.y= Random.Range(-spaceRandomPosition, spaceRandomPosition);
    }
    public void SwitchMoveType()
    {
        if (selectedObjects.Count <= 0) return;
        if (moveType == MoveType.Circle)
        {
            ArrangeSquadToCircle();
            moveType = MoveType.Square;
        }
        else
        {
            ArrangeSquadToSquare();
            moveType = MoveType.Circle;
        }
    }
    public void RemoveAllSelectedList()
    {
        foreach (var selected in selectedObjects) selected.UnSelected();
        selectedObjects.Clear();
    }
    public void SelectAllSoildierOnAreaSelected()
    {
        RemoveAllSelectedList();
        hits = Physics2D.BoxCastAll(rectangleDraw.transform.position, sizeScale, 0, Vector3.forward);
        if(hits.Length == 0&&Vector2.Distance(mousePosBeginHold,InputManager.Instance.mousePoistion)<0.005f)
        {
            hits = Physics2D.RaycastAll(InputManager.Instance.mousePoistion, Vector3.forward);
            foreach (var hit in hits)
            {
                if (hit.collider.transform.TryGetComponent<ISelected>(out ISelected selected))
                {
                    selectedObjects.Add(selected);
                    selected.Selected();
                    SetActiveSelectedArea(false);
                    return;
                }
            }
        }
        AddHitsISelected();
    }
    private void AddHitsISelected()
    {
        foreach (var hit in hits)
        {
            if (hit.collider.transform.TryGetComponent<ISelected>(out ISelected selected))
            {
                selectedObjects.Add(selected);
                selected.Selected();
            }
        }
        SetActiveSelectedArea(false);
    }
    public void SetActiveSelectedArea(bool status)
    {
        rectangleDraw.SetActive(status);
    }
}