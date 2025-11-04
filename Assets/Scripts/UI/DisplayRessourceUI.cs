using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayRessourceUI : MonoBehaviour
{
    RessourceAndAmount _ressourceAndAmount;
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] GameObject _weightAndValueContainer;
    [SerializeField] TextMeshProUGUI _weight;
    [SerializeField] TextMeshProUGUI _value;
    [SerializeField] GameObject _recipeContainer;
    [SerializeField] RectTransform _contentRectTransform;
    [SerializeField] RessourceAndAmountUI[] _ressourcesAndAmountUI;
    [SerializeField] TextMeshProUGUI _description;
    [SerializeField] Vector2 _offSet = new Vector2(5, 5);
    bool _open = false;
    RectTransform _rectTransform;
    UserInterfaceManager _userInterfaceManager;
    Player _player;
    Vector2 _directionOffSet;
    Vector2 _wholeOffSet;

    void Awake()
    {
        _rectTransform = transform as RectTransform;
        _userInterfaceManager = GameManager.instance.userInterfaceManager;

        _player = GameManager.instance.player;
    }

    void Update()
    {
        RefreshPosition();
    }

    void OnEnable()
    {
        _player.onShowRawRecipe += ShowRecipe;
    }
    void OnDisable()
    {
        _player.onShowRawRecipe -= ShowRecipe;
    }
    public void Refresh()
    {
        _name.text = _ressourceAndAmount.ressourceSO.actualName;
        _name.color = GameManager.instance.userInterfaceManager.quality[(int)_ressourceAndAmount.ressourceSO.quality];

        if (_ressourceAndAmount.weight == 0 || _ressourceAndAmount.value == 0)
        {
            _weightAndValueContainer.SetActive(false);
        }
        else
        {
            _weight.text = _ressourceAndAmount.weight + "";
            _value.text = _ressourceAndAmount.value + "";
        }

        _description.text = _ressourceAndAmount.ressourceSO.description;

        ShowRecipe();
    }

    void ShowRecipe(RessourceAndAmount[] recipe)
    {
        _recipeContainer.SetActive(recipe.Length > 0);
        if (!_recipeContainer.activeSelf) return;

        int recipeLength = recipe.Length;

        for (int i = 0; i < _ressourcesAndAmountUI.Length; i++)
        {
            RessourceAndAmountUI ressourceAndAmountUI = _ressourcesAndAmountUI[i];

            // if we still have some ressource to show
            if (i < recipeLength)
            {
                RessourceAndAmount ingredient = recipe[i];

                // show all the ingredient for the ressource
                ressourceAndAmountUI.DisplayRessourceAndAmount(ingredient);
                ressourceAndAmountUI.gameObject.SetActive(true);
            }
            else
            {
                // when we found one that was unactive, we know that the other are
                // also inactive
                if (!ressourceAndAmountUI.gameObject.activeSelf) break;
                ressourceAndAmountUI.gameObject.SetActive(false);
            }
        }
    }

    void ShowRecipe()
    {
        if (_player.showRawRecipeInput) ShowRecipe(_ressourceAndAmount.ressourceSO.rawRessourceToMakeSelf);
        else if (_ressourceAndAmount.ressourceSO.recipe != null) ShowRecipe(_ressourceAndAmount.ressourceSO.recipe.ingredientsInput);
        else ShowRecipe(new RessourceAndAmount[0]);
    }

    public void Display(bool open, RessourceAndAmount ressourceAndAmount = null, Vector2 direction = default)
    {
        _open = open;
        gameObject.SetActive(_open);

        if (_open)
        {
            _ressourceAndAmount = ressourceAndAmount;
            _directionOffSet = direction;

            _wholeOffSet = new Vector2(Mathf.Abs(direction.x - 1) * .5f, (direction.y + 1) * .5f);
            Debug.Log(_directionOffSet + " " + _wholeOffSet);

            Refresh();
            RefreshPosition();
        }
    }

    void RefreshPosition()
    {
        // Convert screen position to local position within the Canvas's RectTransform
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_userInterfaceManager.mainCanvas.transform as RectTransform, Input.mousePosition, null, out Vector2 localPoint))
        {
            Vector2 wholeOffset = _wholeOffSet * new Vector2(_contentRectTransform.rect.width, _contentRectTransform.rect.height);
            Vector2 offset = _directionOffSet * _offSet;
            _rectTransform.anchoredPosition = localPoint + offset + wholeOffset + _userInterfaceManager.screenOffSetNeeded;
        }
    }
}
