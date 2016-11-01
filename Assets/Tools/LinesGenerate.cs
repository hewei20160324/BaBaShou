using UnityEngine;
using System.Collections;
using CustomUtil;

public class LinesGenerate : MonoBehaviour {

    GameObject templeteObj = null;
    GameObject parentObj = null;

    [HideInInspector]
    public bool genereateBoard = false;

    public float rect_length = 96;
    public int horizontal_rect_cnt = 12;
    public float start_y = -89;

    public int line_width = 3;
    public int game_height = 1700;

	// Use this for initialization
	void Start () {
        templeteObj = UnityCustomUtil.GetChild(gameObject, "templete");
        if (templeteObj != null) templeteObj.SetActive(false);

        parentObj = UnityCustomUtil.GetChild(gameObject, "parent");

        genereateBoard = false;
    }
	
	// Update is called once per frame
	void Update () {
	    if( genereateBoard == true || templeteObj == null ) { return; }
        genereateBoard = true;

        UnityCustomUtil.DeleteAllChildren(parentObj);

        UISprite sprite = null;
        GameObject gameObj = null;
        Vector3 tempVec = Vector3.zero;

        // generate vertical lines;
        for (int nIdx = 0; nIdx <= horizontal_rect_cnt; nIdx++)
        {
            gameObj = instantiateTempObj();
            if (gameObj == null) return;

            sprite = gameObj.GetComponent<UISprite>();
            if (sprite == null) return;

            sprite.width = line_width;
            sprite.height = game_height;

            tempVec.y = start_y;
            tempVec.x = rect_length * (nIdx - horizontal_rect_cnt / 2);
            gameObj.transform.localPosition = tempVec;
        }

        // generate horizontal lines;
        int fixLength = 0;
        int nIndex = 0;
        while(fixLength < game_height)
        {
            gameObj = instantiateTempObj();
            if (gameObj == null) return;

            sprite = gameObj.GetComponent<UISprite>();
            if (sprite == null) return;

            sprite.width = (int)rect_length * horizontal_rect_cnt;
            sprite.height = line_width;

            tempVec.x = 0;
            tempVec.y = nIndex * rect_length + start_y - game_height/2;

            gameObj.transform.localPosition = tempVec;

            nIndex++;
            fixLength += (int)rect_length;
        }
    }

    GameObject instantiateTempObj()
    {
        if (templeteObj == null) return null;
        GameObject gameObj = GameObject.Instantiate(templeteObj) as GameObject;
        if (gameObj == null) return null;

        gameObj.SetActive(true);
        gameObj.transform.parent = parentObj != null ? parentObj.transform : transform;
        gameObj.transform.localScale = Vector3.one;
        gameObj.transform.localRotation = Quaternion.identity;
        return gameObj;
    }
}
