using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ToolBarUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  [SerializeField] DisplayBuildingToolBarUI _buildingToolBar;
  [SerializeField] GameObject _strip;
  [SerializeField] float _timeAnimation = .1f;
  Timer _animationTimer;
  Coroutine _refreshAnimation = null;
  bool _open = false;
  private RectTransform _rectTransform;
  Vector2 _awakePosition;

  void Awake()
  {
    _rectTransform = transform as RectTransform;
    _animationTimer = new Timer(_timeAnimation);
    _awakePosition = _rectTransform.anchoredPosition;
  }
  public void OnPointerEnter(PointerEventData eventData)
  {
    if (_refreshAnimation != null) StopCoroutine(_refreshAnimation);
    _refreshAnimation = StartCoroutine(Display(true));
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    if (_refreshAnimation != null) StopCoroutine(_refreshAnimation);
    _refreshAnimation = StartCoroutine(Display(false));
  }

  IEnumerator Display(bool open)
  {
    _open = open;

    _animationTimer.Restart();
    Vector2 startPosition = _rectTransform.anchoredPosition;
    Vector2 targetPosition = _awakePosition;

    if (_open) targetPosition.y += _rectTransform.sizeDelta.y;

    while (!_animationTimer.IsOver())
    {
      Vector2 newPosition = Vector2.zero;
      newPosition.x = Mathf.Lerp(startPosition.x, targetPosition.x, _animationTimer.PercentTime());
      newPosition.y = Mathf.Lerp(startPosition.y, targetPosition.y, _animationTimer.PercentTime());
      _rectTransform.anchoredPosition = newPosition;

      yield return null;
    }

    yield return null;

    _strip.SetActive(!_open);

    _rectTransform.anchoredPosition = targetPosition;
  }

  public void SelectBuilding(BuildingSO buildingSO)
  {
    GameManager.instance.buildingManager.buildingSOToolBarSelected = buildingSO;
  }
  public void DeselectBuilding()
  {
    GameManager.instance.buildingManager.buildingSOToolBarSelected = null;
  }
}
