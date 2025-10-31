using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayRessourceUI : MonoBehaviour
{
    RessourceAndAmount _ressourceAndAmount;
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] TextMeshProUGUI _weight;
    [SerializeField] TextMeshProUGUI _value;
    [SerializeField] GameObject _recipeContainer;
    [SerializeField] RessourceAndAmountUI[] _ressourcesAndAmountUI;
    [SerializeField] TextMeshProUGUI _description;
    [SerializeField] Color[] _quality;
    [SerializeField] Vector2 _offSet = new Vector2(5, 5);
    bool _open = false;
    RectTransform _rectTransform;
    UserInterfaceManager _userInterfaceManager;
    Player _player;

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
        _name.color = _quality[(int)_ressourceAndAmount.ressourceSO.quality];
        _weight.text = _ressourceAndAmount.weight + "";
        _value.text = _ressourceAndAmount.value + "";

        _description.text = _ressourceAndAmount.ressourceSO.description;

        ShowRecipe();
    }

    void ShowRecipe(RessourceAndAmount[] recipe)
    {
        _recipeContainer.SetActive(_ressourceAndAmount.ressourceSO.recipe != null);
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
        else ShowRecipe(_ressourceAndAmount.ressourceSO.recipe.ingredientsInput);
    }

    public void Display(bool open, RessourceAndAmount ressourceAndAmount)
    {
        if (!open && _ressourceAndAmount.ressourceSO != ressourceAndAmount.ressourceSO) return;

        _open = open;

        gameObject.SetActive(_open);
        _ressourceAndAmount = ressourceAndAmount;
        if (_open)
        {
            Refresh();
            RefreshPosition();
        }
    }

    void RefreshPosition()
    {
        // Convert screen position to local position within the Canvas's RectTransform
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_userInterfaceManager.mainCanvas.transform as RectTransform, Input.mousePosition, null, out Vector2 localPoint))
        {
            _rectTransform.anchoredPosition = localPoint + _offSet + _userInterfaceManager.screenOffSetNeeded;
        }
    }
}
