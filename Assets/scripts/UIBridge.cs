using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIBridge : MonoBehaviour
{
    public UnityEngine.UIElements.Image faceSprite;
    public GameObject[] roster;
    public GameObject[] abilityButtons;
    public TextMeshProUGUI[] abilityTexts;

    public UnityEngine.UIElements.Image face_image;
    public GameObject faceObj;
    
    void Start(){
        faceObj.SetActive(false);

        for(int i = 0; i < abilityButtons.Length; i++){
            abilityButtons[i].SetActive(false);
        }
    }

    public void DisableFaceSprite(){
        faceObj.SetActive(false);
    }
    public void ChangeFaceSprite(Sprite faceSprite){
        //face_sprite
        if(faceSprite != null){
            faceObj.SetActive(true);
            face_image.sprite = faceSprite;
        }
        else{
            faceObj.SetActive(false);
        }
    }
}
