using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIBridge : MonoBehaviour
{
    public UnityEngine.UIElements.Image faceSprite;
    public GameObject[] buttons;
    public GameObject clock;
    public TextMeshProUGUI[] abilityTexts;


    [System.Serializable]
    public struct RosterSetup{
        public GameObject cameraButton;
        public GameObject moveImage;
        public GameObject towerImage;
    }

    [SerializeField] public RosterSetup[] rosterUISetup;

    public UnityEngine.UIElements.Image face_image;
    public GameObject faceObj;    
    private void OnEnable()
    {
    }
    
    void Start(){
        faceObj.SetActive(false);
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
