using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIBridge : MonoBehaviour
{

    public GameObject[] abilityButtons;
    public TextMeshProUGUI[] abilityTexts;

    
    void Start(){
        for(int i = 0; i < abilityButtons.Length; i++){
            abilityButtons[i].SetActive(false);
        }
    }
}
