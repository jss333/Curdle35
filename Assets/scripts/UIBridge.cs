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
            uiRoster[i].healthBar.GetComponent<UnityEngine.UI.Image>().sprite = uiRoster[i].sprites[healths[i]];

            if(healths[i] > uiRoster[i].sprites.Length){
                Debug.Log("cat has more health than planned : " + i);
            }
        }

        scoreTexts[0].text = catScore.ToString();
        scoreTexts[1].text = hyenaScore.ToString();
    }
}
