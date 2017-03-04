using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(ListListPass))]
public class ListListPassEditor : PropertyDrawer
{
    ListPass temp;
    GUIContent buttonInfo;
    Vector2 buttonSize;

    void OnEnable()
    {
        temp = new ListPass();
        buttonSize = Vector2.one * 100;
        buttonInfo = new GUIContent();
        buttonInfo.image = (Texture)Resources.Load("addButton");
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (buttonInfo == null)
        {
            buttonInfo = new GUIContent();
            buttonInfo.image = (Texture)Resources.Load("addButton");
            buttonSize = Vector2.one * 15;
        }
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.indentLevel = 1;
        float indentConst = 15;
        if (GUI.Button(new Rect(position.position - Vector2.right * buttonSize.x + Vector2.right * EditorGUI.indentLevel * indentConst, buttonSize), "v") && property.FindPropertyRelative("size").intValue < property.FindPropertyRelative("capacity").intValue++)
        {
            property.FindPropertyRelative("lcontent").InsertArrayElementAtIndex(property.FindPropertyRelative("size").intValue);
            property.FindPropertyRelative("size").intValue++;
        }
        bool remove;
        bool add;
        EditorGUI.LabelField(position,label);
        EditorGUILayout.BeginVertical();
        for (int i = 0; i < property.FindPropertyRelative("size").intValue; i++)
        {
            position = EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel = 1;
            remove = GUI.Button(new Rect(position.position - Vector2.right * buttonSize.x + Vector2.right * EditorGUI.indentLevel * indentConst, buttonSize), "^");
            add = GUI.Button(new Rect(position.position - Vector2.right * buttonSize.x + Vector2.right * (EditorGUI.indentLevel - 1) * indentConst, buttonSize), "v");
            if (add)
            {
                property.FindPropertyRelative("lcontent").InsertArrayElementAtIndex(i);
                property.FindPropertyRelative("size").intValue++;
            }
            if (!remove)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("lcontent").GetArrayElementAtIndex(i));
            }
            else
            {
                property.FindPropertyRelative("size").intValue--;
                property.FindPropertyRelative("lcontent").DeleteArrayElementAtIndex(i);
                i--;
            }
            EditorGUILayout.EndVertical();

        }
        EditorGUI.indentLevel = 1;

        EditorGUILayout.EndVertical();
        EditorGUI.EndProperty();
    }

    /*public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(passType);
        if(passType.enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(intPass);
        }
        else
        {
            EditorGUILayout.PropertyField(vecPass);
        }

        serializedObject.ApplyModifiedProperties();
    }*/
}

[CustomPropertyDrawer(typeof(ListPass))]
public class ListPassEditor : PropertyDrawer
{
    ListPass temp;
    GUIContent buttonInfo;
    Vector2 buttonSize;

    void OnEnable()
    {
        temp = new ListPass();
        buttonSize = Vector2.one * 100;
        buttonInfo = new GUIContent();
        buttonInfo.image = (Texture)Resources.Load("addButton");
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (buttonInfo == null)
        {
            buttonInfo = new GUIContent();
            buttonInfo.image = (Texture)Resources.Load("addButton");
            buttonSize = Vector2.one * 15;
        }
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.indentLevel = 2;
        float indentConst = 15;
        if (GUI.Button(new Rect(position.position - Vector2.right * buttonSize.x + Vector2.right * EditorGUI.indentLevel * indentConst, buttonSize), "+") && property.FindPropertyRelative("size").intValue < property.FindPropertyRelative("capacity").intValue++)
        {
            property.FindPropertyRelative("content").InsertArrayElementAtIndex(property.FindPropertyRelative("size").intValue);
            property.FindPropertyRelative("size").intValue++;
        }
        bool remove;
        bool add;
        EditorGUI.LabelField(position,label);
        EditorGUILayout.BeginVertical();
        for (int i = 0; i < property.FindPropertyRelative("size").intValue; i++)
        {
            position = EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel = 3;
            remove = GUI.Button(new Rect(position.position - Vector2.right * buttonSize.x + Vector2.right * EditorGUI.indentLevel * indentConst, buttonSize), "-");
            add = GUI.Button(new Rect(position.position - Vector2.right * buttonSize.x + Vector2.right * (EditorGUI.indentLevel - 1) * indentConst, buttonSize), "+");
            if (add)
            {
                property.FindPropertyRelative("content").InsertArrayElementAtIndex(i);
                property.FindPropertyRelative("size").intValue++;
            }
            if (!remove)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("content").GetArrayElementAtIndex(i));
            }
            else
            {
                property.FindPropertyRelative("size").intValue--;
                property.FindPropertyRelative("content").DeleteArrayElementAtIndex(i);
                i--;
            }
            EditorGUILayout.EndVertical();

        }
        EditorGUI.indentLevel = 2;

        EditorGUILayout.EndVertical();
        EditorGUI.EndProperty();
    }

    /*public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(passType);
        if(passType.enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(intPass);
        }
        else
        {
            EditorGUILayout.PropertyField(vecPass);
        }

        serializedObject.ApplyModifiedProperties();
    }*/
}

[CustomPropertyDrawer(typeof(Pass))]
public class PassEditor : PropertyDrawer
{
    SerializedProperty passType;
    SerializedProperty vecPass;
    SerializedProperty intPass;

    void OnEnable()
    {
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position,label);
        float y = 20;
        position.position = position.position + Vector2.up * y;
        EditorGUILayout.PropertyField(property.FindPropertyRelative("type"));
        if(property.FindPropertyRelative("type").enumValueIndex == 0)
        {
            position.position = position.position + Vector2.up * y;
            EditorGUILayout.PropertyField(property.FindPropertyRelative("position"));

        }
        else
        {
            position.position = position.position + Vector2.up * y;
            EditorGUILayout.PropertyField(property.FindPropertyRelative("location"));
        }
        EditorGUI.EndProperty();
    }

    /*public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(passType);
        if(passType.enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(intPass);
        }
        else
        {
            EditorGUILayout.PropertyField(vecPass);
        }

        serializedObject.ApplyModifiedProperties();
    }*/
}



[CustomEditor(typeof(Strategy))]
public class StrategyEditor : Editor {

    SerializedProperty strategyType;
    SerializedProperty numOfPlayers;
    SerializedProperty courtPositions;
    SerializedProperty defensePassLocation;
    SerializedProperty pass;
    Texture courtImage;
    Texture ballImage;
    Vector2[] players;
    Vector2[] oldPlayers;
    Texture[] playerPics;

    GUIContent buttonInfo;
    Vector2 buttonSize;

    Vector2 position;
    Vector2 size;
    Vector2 ballSize;
    Vector2 playerSize;
    float y;
	// Use this for initialization
	void Start () {
		
	}

    void OnEnable()
    {
        
        strategyType = serializedObject.FindProperty("type");
        numOfPlayers = serializedObject.FindProperty("numOfPlayers");
        courtPositions = serializedObject.FindProperty("defaultPositions");
        defensePassLocation = serializedObject.FindProperty("defensePassToLocation");
        pass = serializedObject.FindProperty("myPass");
        InitializePlayers();
        InitializeCourt();
        buttonSize = Vector2.one * 25;
        buttonInfo = new GUIContent();
        buttonInfo.image = (Texture)Resources.Load("AddButton");
        

    }

    void InitializePlayers()
    {
        players = new Vector2[6];
        playerPics = new Texture[players.Length];
        oldPlayers = new Vector2[players.Length];
        
        if(courtPositions.arraySize != 6)
        {
            MonoBehaviour.print(courtPositions.arraySize);
            for (int i = courtPositions.arraySize; i < 6; i++)
                courtPositions.InsertArrayElementAtIndex(i);
            for (int i = courtPositions.arraySize; i > 6; i--)
                courtPositions.DeleteArrayElementAtIndex(i - 1);
            for (int i = 0; i < players.Length; i++)
            {
                int position = (i + 5) % 6;
                int horizPosition = position % 3;
                float depth = position < 3 ? 15 / 90f : 60 / 90f;
                float hPosition = -horizPosition * 30 / 90f + 30 / 90f;
                hPosition = position < 3 ? hPosition : -hPosition;
                players[i] = new Vector2(hPosition, depth);
                playerPics[i] = (Texture2D)Resources.Load("player" + (i + 1).ToString());
                courtPositions.GetArrayElementAtIndex(i).vector3Value = V2toV3(players[i]);
            }

        }
        else
        {
            for(int i = 0; i < courtPositions.arraySize; i++)
            {
                players[i] = V3toV2(courtPositions.GetArrayElementAtIndex(i).vector3Value);
                playerPics[i] = (Texture2D)Resources.Load("player" + (i+1).ToString());
            }
        }
        playerSize = Vector2.one * 40;
        serializedObject.ApplyModifiedProperties();
    }

    void InitializeCourt()
    {
        position = new Vector2(55, 100);
        size = new Vector2(450, 450);
        ballSize = Vector2.one * 25;
        courtImage = (Texture)Resources.Load("court");
        ballImage = (Texture)Resources.Load("ball");
    }

    

    public override void OnInspectorGUI()
    {
        y = 0;
        serializedObject.Update();
        EditorGUILayout.PropertyField(strategyType);
        numOfPlayers.intValue = EditorGUILayout.IntField(numOfPlayers.intValue);
        AddY(100);
        
        position.y = y;
        GUI.DrawTexture(new Rect(position,size), courtImage);
        AddY(size.y + 25);
        GUILayout.Space(size.y + 25);
        
        //draws the players and stores value into actual variable;
        for(int i = 0; i < numOfPlayers.intValue; i++)
        {
            players[i] = Vector2.Scale(EditorGUILayout.Vector2Field("Player" + (i+1).ToString(), Vector2.Scale(players[i],Vector2.one * 9)),Vector2.one * 1/9);
            GUI.DrawTexture(new Rect(Vector2.Scale(players[i], size) + PlaceOnCourt(playerSize), playerSize), playerPics[i]);
            if (oldPlayers[i] != players[i])
            {
                oldPlayers[i] = players[i];
                courtPositions.GetArrayElementAtIndex(i).vector3Value = V2toV3(players[i]);
            }
            AddY();
        }
        
        //drawing for offense
        if (strategyType.enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(pass);
        }
        //drawing for defense
        else
        {
            //GUI.DrawTexture(new Rect(Vector2.Scale(V3toV2(defensePassLocation.vector3Value), size) + PlaceOnCourt(ballSize), ballSize), ballImage);
        }
        
        serializedObject.ApplyModifiedProperties();
        //GUI.Button(new Rect(new Vector2(10,y), buttonSize), buttonInfo);
    }

    void AddY(float additive = 20f)
    {
        y += additive;
    }

    Vector3 V2toV3(Vector2 temp)
    {
        Vector2.Scale(temp, Vector2.one * 9);
        return new Vector3(temp.x, 1.1f, temp.y);
    }
    
    Vector2 V3toV2(Vector3 temp)
    {
        Vector3.Scale(temp, Vector3.one * 1/9);
        return new Vector2(temp.x, temp.z);
    }

    Vector2 PlaceOnCourt(Vector2 imageSize)
    {
        return position + Vector2.right * size.x/2 - imageSize/2;
    }
}
