using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIBridge : MonoBehaviour
{
    //public UnityEngine.UIElements.Image face_image;
    public GameObject[] buttons;

    public TextMeshProUGUI towerPlacementText;
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

    public UnityEngine.UI.Image face_image;
    private void OnEnable()
    {
    }
    
    void Start(){
        face_image.enabled = false;
        foreach(var button in buttons){
            button.SetActive(true);
        }
    }

    public void DisableFaceSprite(){
        //faceObj.SetActive(false);
    }
    public void ChangeFaceSprite(Sprite faceSprite){
        //face_sprite
        if(faceSprite != null){
            face_image.enabled = true;
            face_image.sprite = faceSprite;
        }
        else{
            face_image.enabled = false;
        }
    }

    public void UpdateUI(int[] healths, Vector2Int catScore, Vector2Int hyenaScore) {
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

        scoreTexts[0].text = catScore.x.ToString();
        scoreTexts[1].text = hyenaScore.x.ToString();
        scoreTexts[2].text = catScore.y.ToString();
        scoreTexts[3].text = hyenaScore.y.ToString();
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
    public void SetEndTurnButtonActivity(bool active){
        //this doesnt fully disable it just changes the color
        if(active){
            buttons[2].GetComponent<UnityEngine.UI.Image>().color = buttonEnabledColor;
        }
        else{
            buttons[2].GetComponent<UnityEngine.UI.Image>().color = buttonDisabledColor;
        }
    }
}
