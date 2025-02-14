using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIBridge : MonoBehaviour
{
    public UnityEngine.UIElements.Image faceSprite;
    public GameObject[] buttons;
    public GameObject clock;
    //public TextMeshProUGUI[] abilityTexts;
    public TextMeshProUGUI[] scoreTexts;

    [SerializeField] public Color buttonDisabledColor;
    [SerializeField] public Color buttonEnabledColor;




    [System.Serializable]
    public struct RosterSetup{

        public GameObject healthBar;
        public Sprite[] sprites;
    }

    [SerializeField] private RosterSetup[] uiRoster;

    public UnityEngine.UIElements.Image face_image;
    public GameObject faceObj;    
    private void OnEnable()
    {
    }
    
    void Start(){
        faceObj.SetActive(false);
        foreach(var button in buttons){
            button.SetActive(true);
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

    public void UpdateUI(int[] healths, int catScore, int hyenaScore) {
        for(int i = 0; i < healths.Length; i++){
            if(healths[i] < 0){
                uiRoster[i].healthBar.GetComponent<UnityEngine.UI.Image>().sprite = uiRoster[i].sprites[0];
                Debug.Log("hp is out of bounds : " + uiRoster[i].sprites.Length + ":" + healths[i]);
            }
            else if(healths[i] >= uiRoster[i].sprites.Length){
                uiRoster[i].healthBar.GetComponent<UnityEngine.UI.Image>().sprite = uiRoster[i].sprites[uiRoster[i].sprites.Length - 1];
                Debug.Log("hp is out of bounds : " + uiRoster[i].sprites.Length + ":" + healths[i]);
            }
            else{
                uiRoster[i].healthBar.GetComponent<UnityEngine.UI.Image>().sprite = uiRoster[i].sprites[healths[i]];
            }
        }

        scoreTexts[0].text = catScore.ToString();
        scoreTexts[1].text = hyenaScore.ToString();
    }

    public void SetMoveButtonActivity(bool active){
        //this doesnt fully disable it just changes the color
        if(active){
            buttons[0].GetComponent<UnityEngine.UI.Image>().color = buttonEnabledColor;
        }
        else{
            buttons[0].GetComponent<UnityEngine.UI.Image>().color = buttonDisabledColor;
        }
    }
    public void SetTowerPlacementButtonActivity(bool active){
        //this doesnt fully disable it just changes the color
        if(active){
            buttons[1].GetComponent<UnityEngine.UI.Image>().color = buttonEnabledColor;
        }
        else{
            buttons[1].GetComponent<UnityEngine.UI.Image>().color = buttonDisabledColor;
        }
    }
}
