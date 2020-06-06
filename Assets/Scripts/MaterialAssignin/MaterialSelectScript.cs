using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Stick this guy onto an empty gameobject and it will let you assign buttons to go along with materials for use in the level
public class MaterialSelectScript : MonoBehaviour
{
    //This class allows you to have multiple elements assigned to a list, I find it
    // easier to work with in the editor than creating sperate lists
    [System.Serializable]
    public class MaterialButtons
    {
        public Button button;
        public Material material;

        private bool selected = false;
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }
    }

    public GameObject panelObj;
    public RectTransform panelSpace;
    public List<MaterialButtons> buttonList;

    private void Start()
    {
        panelObj.SetActive(false);

    }

}
