using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(ListListPass))]
public class ListListPassEditor : PropertyDrawer
{
    GUIContent buttonInfo;
    Vector2 buttonSize;

    void OnEnable()
    {
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
        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(ListPass))]
public class ListPassEditor : PropertyDrawer
{
    GUIContent buttonInfo;
    Vector2 buttonSize;
    Dictionary<string, bool> fold;
    string myLabel;
    //bool fold = true;

    void OnEnable()
    {
        buttonSize = Vector2.one * 100;
        buttonInfo = new GUIContent();
        buttonInfo.image = (Texture)Resources.Load("addButton");
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        myLabel = label.text;
        if (buttonInfo == null)
        {
            buttonInfo = new GUIContent();
            buttonInfo.image = (Texture)Resources.Load("addButton");
            buttonSize = Vector2.one * 15;
        }

        if (fold == null)
        {
            fold = new Dictionary<string, bool>();
            fold.Add(myLabel, false);
        }
        if (!fold.ContainsKey(myLabel))
        {
            fold.Add(myLabel, false);
        }
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.indentLevel += 2;//EditorGUI.indentLevel = 3;
        float indentConst = 15;
        if (GUI.Button(new Rect(position.position - Vector2.right * buttonSize.x + Vector2.right * (EditorGUI.indentLevel - 1) * indentConst, buttonSize), "+") && property.FindPropertyRelative("size").intValue < property.FindPropertyRelative("capacity").intValue++)
        {
            property.FindPropertyRelative("content").InsertArrayElementAtIndex(property.FindPropertyRelative("size").intValue);
            property.FindPropertyRelative("size").intValue++;
        }
        bool remove;
        bool add;
        fold[myLabel] = EditorGUI.Foldout(position, fold[myLabel], label);
        EditorGUILayout.BeginVertical();
        if (fold[myLabel])
        {
            EditorGUI.indentLevel+= 1;
            for (int i = 0; i < property.FindPropertyRelative("size").intValue; i++)
            {
                position = EditorGUILayout.BeginVertical();
                //EditorGUI.indentLevel = 4;
                remove = GUI.Button(new Rect(position.position - Vector2.right * buttonSize.x + Vector2.right * (EditorGUI.indentLevel) * indentConst, buttonSize), "-");
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
            EditorGUI.indentLevel -= 2;
        }
        //EditorGUI.indentLevel = 3;
        EditorGUI.indentLevel -= 2; 
        EditorGUILayout.EndVertical();
        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(Pass))]
public class PassEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        //EditorGUI.BeginChangeCheck();
        EditorGUI.PrefixLabel(position, label);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("visible"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("type"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("speed"));
        if (property.FindPropertyRelative("type").enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("position"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("offset"));
            if (property.FindPropertyRelative("position").intValue < 1)
                property.FindPropertyRelative("position").intValue = 1;
            if (property.FindPropertyRelative("position").intValue > CourtScript.MaxNumberOfPlayers)
                property.FindPropertyRelative("position").intValue = CourtScript.MaxNumberOfPlayers;
        }
        else
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("location"));
        }
        
        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(Path))]
public class PathEditor : PropertyDrawer
{
    Dictionary<string, Vector2[]> path;
    //int index = 0;
    //int size;
    bool delete;
    Dictionary<string, int> index;
    Dictionary<string, int> size;
    string myLabel;
    Texture fast;
    Texture slow;
    int arraySize;
    Dictionary<string,Texture> image;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        myLabel = label.text;
        InitializeVariables(property);
        EditorGUI.BeginProperty(position, label, property);
        if (property.FindPropertyRelative("size").intValue > 0)
        {
            EditorGUI.indentLevel = 1;
            if(size[myLabel] != property.FindPropertyRelative("size").intValue)
            {
                size[myLabel] = property.FindPropertyRelative("size").intValue;
                index[myLabel] = size[myLabel] - 1;
            }
    
            position = EditorGUI.PrefixLabel(position, label);
            position.position = new Vector2(15 + EditorGUI.indentLevel * 15,position.position.y);
            if (GUI.Button(new Rect(position.position - Vector2.right * 15 * EditorGUI.indentLevel, Vector2.one * 15), "-"))
            {
                DeleteElement(property.FindPropertyRelative("points"), index[myLabel]);
                DeleteElement(property.FindPropertyRelative("walkToThisPoint"), index[myLabel]);
                property.FindPropertyRelative("size").intValue--;
                UpdatePathPoints(property.FindPropertyRelative("points"));
                size[myLabel] = property.FindPropertyRelative("size").intValue;
                if (size[myLabel] < 1)
                    return;
            }

            index[myLabel] = EditorGUILayout.IntField("Points Index", index[myLabel] + 1)-1;
            if (index[myLabel] >= property.FindPropertyRelative("size").intValue)
                index[myLabel] = property.FindPropertyRelative("size").intValue - 1;
            else if (index[myLabel] < 0)
                index[myLabel] = 0;
            position.position += Vector2.up * (15 * 2 + 5);
            
            EditorGUILayout.PropertyField(property.FindPropertyRelative("walkToThisPoint").GetArrayElementAtIndex(index[myLabel]));
            image[myLabel] = property.FindPropertyRelative("walkToThisPoint").GetArrayElementAtIndex(index[myLabel]).boolValue ? fast : slow;
            GUI.DrawTexture(new Rect(position.position,Vector2.Scale(position.size,new Vector2(.5f,1))), image[myLabel]);
            position = EditorGUI.PrefixLabel(new Rect(position.position - Vector2.right * 15,position.size), new GUIContent("Walk To This Point"));
            EditorGUI.BeginChangeCheck();
            path[myLabel][index[myLabel]] = EditorGUILayout.Vector2Field("Path Point " + (index[myLabel] + 1).ToString(), path[myLabel][index[myLabel]]);
            if (Mathf.Abs(path[myLabel][index[myLabel]].x) < 4.5f && path[myLabel][index[myLabel]].y < .5f)
                path[myLabel][index[myLabel]].y = .5f;
            if(EditorGUI.EndChangeCheck())
                property.FindPropertyRelative("points").GetArrayElementAtIndex(index[myLabel]).vector3Value = V2toV3(path[myLabel][index[myLabel]]);

            EditorGUILayout.PropertyField(property.FindPropertyRelative("shouldJump"));
            if (property.FindPropertyRelative("shouldJump").boolValue)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("stopJump"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("jumpTime"));
                if(property.FindPropertyRelative("jumpTime").enumValueIndex >= 1)
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("timeOffset"));
                }
            }

            EditorGUI.indentLevel = 0;
        }
        
        EditorGUI.EndProperty();
    }

    Vector3 V2toV3(Vector2 temp)
    {
        return new Vector3(temp.x, 1.1f, temp.y);
    }

    Vector2 V3toV2(Vector3 temp)
    {
        return new Vector2(temp.x, temp.z);
    }

    void DeleteElement(SerializedProperty array, int index)
    {
        for(int i = index; i < size[myLabel] - 1; i++)
            array.GetArrayElementAtIndex(i).vector3Value = array.GetArrayElementAtIndex(i + 1).vector3Value;
    }

    void UpdatePathPoints( SerializedProperty array)
    {
        for(int i = 0; i < size[myLabel]; i++)
            path[myLabel][i] = V3toV2(array.GetArrayElementAtIndex(i).vector3Value);
    }

    void InitializeVariables(SerializedProperty property)
    {
        arraySize = 15;
        if (fast == null)
            fast = (Texture)Resources.Load("p3Line");
        if (slow == null)
            slow = (Texture)Resources.Load("p4Line");
        if (image == null)
            image = new Dictionary<string, Texture>();
        if (!image.ContainsKey(myLabel))
            image.Add(myLabel, new Texture());
        if (index == null)
        {
            index = new Dictionary<string, int>();
            index.Add(myLabel, 0);
        }
        if (!index.ContainsKey(myLabel))
            index.Add(myLabel, 0);
        if (size == null)
        {
            size = new Dictionary<string, int>();
            size.Add(myLabel, 0);
        }
        if (!size.ContainsKey(myLabel))
            size.Add(myLabel, 0);
        if (path == null || !path.ContainsKey(myLabel) || property.FindPropertyRelative("points").arraySize != arraySize)
            InitializeArray(property.FindPropertyRelative("points"), arraySize);
        if (property.FindPropertyRelative("walkToThisPoint").arraySize != arraySize)
            InitializeArray(property.FindPropertyRelative("walkToThisPoint"), arraySize);
    }

    void InitializeArray(SerializedProperty property, int size)
    {
        if (path == null)
        {
            path = new Dictionary<string, Vector2[]>();
            path.Add(myLabel, new Vector2[size]);
        }
        if (!path.ContainsKey(myLabel))
            path.Add(myLabel, new Vector2[size]);

        for (int i = property.arraySize; i < size; i++)
            property.InsertArrayElementAtIndex(i);
        for (int i = property.arraySize; i > size; i--)
            property.DeleteArrayElementAtIndex(i - 1);

        for(int i = 0; i < size; i ++)
            path[myLabel][i] = V3toV2(property.GetArrayElementAtIndex(i).vector3Value);
    }
}



[CustomEditor(typeof(Strategy))]
public class StrategyEditor : Editor {

    SerializedProperty strategyType;
    SerializedProperty numOfPlayers;
    SerializedProperty courtPositions;
    SerializedProperty defensePassLocation;
    SerializedProperty pass;
    SerializedProperty path;
    SerializedProperty block;
    SerializedProperty defensePosition;
    List<SerializedProperty> balls;
    Texture courtImage;
    Texture fenceImage;
    Texture ballImage;
    Texture angleImage;
    Texture seamImage;
    Texture[] Line;
    Vector2[] players;
    Vector2[] oldPlayers;
    Texture[] playerPics;

    GUIContent buttonInfo;
    Vector2 buttonSize;

    Vector2 position;
    Vector2 size;
    Vector2 ballSize;
    Vector2 playerSize;
    Vector2 myPosition;
    Vector2 pointSize;
    Vector2 fenceSize;
    float lineSize = 5;
    float y;
    bool initial = true;
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
        path = serializedObject.FindProperty("movementPath");
        block = serializedObject.FindProperty("blocker");
        defensePosition = serializedObject.FindProperty("myPosition");
        InitializePlayers();
        InitializeCourt();
        buttonSize = Vector2.one * 20;
        buttonInfo = new GUIContent();
        buttonInfo.image = (Texture)Resources.Load("AddButton");
        balls = new List<SerializedProperty>();
        initial = true;
    }

    void InitializePlayers()
    {
        players = new Vector2[CourtScript.MaxNumberOfPlayers];
        playerPics = new Texture[players.Length];
        oldPlayers = new Vector2[players.Length];
        Line = new Texture[players.Length];
        if(path.arraySize != CourtScript.MaxNumberOfPlayers)
        {
            for (int i = path.arraySize; i < CourtScript.MaxNumberOfPlayers; i++)
                path.InsertArrayElementAtIndex(i);
            for (int i = path.arraySize; i > CourtScript.MaxNumberOfPlayers; i--)
                path.DeleteArrayElementAtIndex(i - 1);
        }
        if(courtPositions.arraySize != CourtScript.MaxNumberOfPlayers)
        {
            for (int i = courtPositions.arraySize; i < CourtScript.MaxNumberOfPlayers; i++)
                courtPositions.InsertArrayElementAtIndex(i);
            for (int i = courtPositions.arraySize; i > CourtScript.MaxNumberOfPlayers; i--)
                courtPositions.DeleteArrayElementAtIndex(i - 1);
            for (int i = 0; i < players.Length; i++)
            {
                int position = (i + 5) % CourtScript.MaxNumberOfPlayers;
                int horizPosition = position % 3;
                float depth = position < 3 ? 15 / 90f : 60 / 90f;
                float hPosition = -horizPosition * 30 / 90f + 30 / 90f;
                hPosition = position < 3 ? hPosition : -hPosition;
                players[i] = new Vector2(hPosition, depth);
                playerPics[i] = (Texture2D)Resources.Load("player" + (i + 1).ToString());
                Line[i] = (Texture)Resources.Load("p" + (i + 1).ToString() + "Line");
                courtPositions.GetArrayElementAtIndex(i).vector3Value = V2toV3(players[i]);
            }

        }
        else
        {
            for(int i = 0; i < courtPositions.arraySize; i++)
            {
                players[i] = new Vector2(courtPositions.GetArrayElementAtIndex(i).vector3Value.x, courtPositions.GetArrayElementAtIndex(i).vector3Value.z)/9;
                playerPics[i] = (Texture2D)Resources.Load("player" + (i + 1).ToString());
                Line[i] = (Texture)Resources.Load("p" + (i + 1).ToString() + "Line");
                
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
        pointSize = Vector2.one * 12;
        fenceSize = Vector2.one * 30;
        courtImage = (Texture)Resources.Load("court");
        ballImage = (Texture)Resources.Load("ball");
        fenceImage = (Texture)Resources.Load("fence");
        angleImage = (Texture)Resources.Load("angle");
        seamImage = (Texture)Resources.Load("seam");
    }

    

    public override void OnInspectorGUI()
    {
        y = 0;
        serializedObject.Update();
        EditorGUILayout.PropertyField(strategyType);
        numOfPlayers.intValue = EditorGUILayout.IntField("Number of Players", numOfPlayers.intValue);
        AddY(100);
        position.y = y;
        GUI.DrawTexture(new Rect(position, size), courtImage);
        AddY(size.y + 25);
        GUILayout.Space(size.y + 25);
        
        //draws the players and stores value into actual variable;
        for(int i = 0; i < numOfPlayers.intValue; i++)
        {
            players[i] = Vector2.Scale(EditorGUILayout.Vector2Field("Player" + (i+1).ToString(), Vector2.Scale(players[i],Vector2.one * 9)),Vector2.one * 1/9);
            GUI.DrawTexture(new Rect(Vector2.Scale(players[i], size) + PlaceOnCourt(playerSize), playerSize), playerPics[i]);
            if (strategyType.enumValueIndex == 0)
            {
                if(path.GetArrayElementAtIndex(i).FindPropertyRelative("points").arraySize < 1)
                    EditorGUILayout.PropertyField(path.GetArrayElementAtIndex(i));
                if (GUI.Button(new Rect(Vector2.Scale(players[i], size) + PlaceOnCourt(playerSize) + buttonSize/2, buttonSize), "+"))
                {
                    if (path.GetArrayElementAtIndex(i).FindPropertyRelative("points").arraySize > path.GetArrayElementAtIndex(i).FindPropertyRelative("size").intValue)
                    {
                        path.GetArrayElementAtIndex(i).FindPropertyRelative("size").intValue++;
                    }
                    
                }

                if (path.GetArrayElementAtIndex(i).FindPropertyRelative("size").intValue > 0)
                {
                    EditorGUILayout.PropertyField(path.GetArrayElementAtIndex(i));
                    Vector2 previousPosition = Vector2.Scale(players[i], size) + PlaceOnCourt(playerSize) + playerSize/ 2 - lineSize/2 * Vector2.up;
                    Vector2 distance;
                    Vector2 previousSize = playerSize;
                    float angle;
                    for (int p = 0; p < path.GetArrayElementAtIndex(i).FindPropertyRelative("size").intValue; p++)
                    {
                        myPosition = Vector2.Scale(V3toV2(path.GetArrayElementAtIndex(i).FindPropertyRelative("points").GetArrayElementAtIndex(p).vector3Value), size) + PlaceOnCourt(pointSize);
                        GUI.DrawTexture(new Rect(myPosition, pointSize), playerPics[i]);
                        myPosition += pointSize/2 - lineSize/2 * Vector2.up;
                        distance = myPosition- previousPosition;
                        angle = Mathf.Atan2(distance.y,distance.x) * Mathf.Rad2Deg;
                        GUIUtility.RotateAroundPivot(angle, previousPosition + lineSize/2 * Vector2.up);
                        GUI.DrawTexture(new Rect(previousPosition, new Vector2(distance.magnitude, lineSize)), Line[i]);
                        GUIUtility.RotateAroundPivot(-angle, previousPosition + lineSize/2 * Vector2.up);
                        previousPosition = myPosition;
                        previousSize = pointSize;
                    }
                }

            }
            else
            {
                if(block.arraySize != CourtScript.MaxNumberOfPlayers)
                {
                    for (int b = block.arraySize; b < CourtScript.MaxNumberOfPlayers; b++)
                        block.InsertArrayElementAtIndex(b);
                    for (int b = block.arraySize; b > CourtScript.MaxNumberOfPlayers; b--)
                        block.DeleteArrayElementAtIndex(b - 1);
                }
                if (defensePosition.arraySize != CourtScript.MaxNumberOfPlayers)
                {
                    for (int b = defensePosition.arraySize; b < CourtScript.MaxNumberOfPlayers; b++)
                        defensePosition.InsertArrayElementAtIndex(b);
                    for (int b = defensePosition.arraySize; b > CourtScript.MaxNumberOfPlayers; b--)
                        defensePosition.DeleteArrayElementAtIndex(b - 1);
                }

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(defensePosition.GetArrayElementAtIndex(i), new GUIContent("Defensive Position"));

                EditorGUILayout.PropertyField(block.GetArrayElementAtIndex(i),new GUIContent("Block"));
                EditorGUI.indentLevel--;
                if (defensePosition.GetArrayElementAtIndex(i).enumValueIndex == 0)
                {
                    GUI.DrawTexture(new Rect(Vector2.Scale(players[i], size) + PlaceOnCourt(fenceSize), fenceSize), fenceImage);
                }
                else if(defensePosition.GetArrayElementAtIndex(i).enumValueIndex == 1)
                {
                    GUI.DrawTexture(new Rect(Vector2.Scale(players[i], size) + PlaceOnCourt(fenceSize), fenceSize), angleImage);
                }
                else if(defensePosition.GetArrayElementAtIndex(i).enumValueIndex == 2)
                {
                    GUI.DrawTexture(new Rect(Vector2.Scale(players[i], size) + PlaceOnCourt(fenceSize), fenceSize), seamImage);
                }
            }
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
            //GUI.changed = false;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pass);
            if (EditorGUI.EndChangeCheck() | initial)
            {
                for (int i = 0; i < pass.FindPropertyRelative("size").intValue; i++)
                {
                    for (int b = 0; b < pass.FindPropertyRelative("lcontent").GetArrayElementAtIndex(i).FindPropertyRelative("size").intValue; b++)
                    {
                        SerializedProperty myProp = pass.FindPropertyRelative("lcontent").GetArrayElementAtIndex(i).FindPropertyRelative("content").GetArrayElementAtIndex(b);
                        if (myProp.FindPropertyRelative("visible").boolValue && !balls.Exists(x=> x == myProp))
                        {
                            balls.Add(myProp);
                        }
                        string path = myProp.propertyPath.Remove(myProp.propertyPath.IndexOf(".", myProp.propertyPath.LastIndexOf(".") - 7));
                        //MonoBehaviour.print(path.Substring(myProp.propertyPath.IndexOf(".") +1));
                        //MonoBehaviour.print(myProp.serializedObject.FindProperty(path).name);
                    }
                }
            }
            for(int i = 0; i < balls.Count; i++)
            {
                if (balls[i] != null)
                {//pass to person
                    string index = balls[i].propertyPath.Substring(balls[i].propertyPath.LastIndexOf("[") + 1).Remove(balls[i].propertyPath.Substring(balls[i].propertyPath.LastIndexOf("[") + 1).Length - 1);
                    int begging = balls[i].propertyPath.IndexOf("[");
                    string rootIndex = balls[i].propertyPath.Substring(begging+1, balls[i].propertyPath.IndexOf("]") - begging-1);
                    int intRootIndex = System.Convert.ToInt32(rootIndex) + 1;
                    int mySize = balls[i].serializedObject.FindProperty(balls[i].propertyPath.Substring(0, balls[i].propertyPath.IndexOf(".", 10))).GetArrayElementAtIndex(System.Convert.ToInt32(rootIndex)).FindPropertyRelative("size").intValue;
                    if(System.Convert.ToInt32(index) >= mySize)
                    {
                        balls.RemoveAt(i);
                        i--;
                        continue;
                    }

                    if (balls[i].FindPropertyRelative("type").enumValueIndex == 0)
                    {
                        try
                        {
                            Vector2 ballPosition = Vector2.Scale(players[balls[i].FindPropertyRelative("position").intValue - 1], size) + PlaceOnCourt(ballSize) + Vector2.Scale(V3toV2(balls[i].FindPropertyRelative("offset").vector3Value), size);
                            GUI.DrawTexture(new Rect(ballPosition, ballSize), ballImage);
                            GUIStyle me = new GUIStyle();
                            me.normal.textColor = Color.white;
                            GUIContent txt = new GUIContent((System.Convert.ToInt32(rootIndex) + 1).ToString(), "Passed to location");
                            GUI.Label(new Rect(ballPosition + ballSize/4, ballSize), txt, me);
                            
                            //GUI.contentColor = old;
                            //MonoBehaviour.print(index + ", " + rootIndex + ",. "+ balls[i].serializedObject.FindProperty(balls[i].propertyPath.Substring(0, balls[i].propertyPath.IndexOf(".",10))).GetArrayElementAtIndex(System.Convert.ToInt32(rootIndex)).FindPropertyRelative("size").intValue);
                        }
                        catch (System.Exception e)
                        {
                            MonoBehaviour.print("error " + balls[i].propertyPath.Substring(0, balls[i].propertyPath.IndexOf(".",10)));
                            MonoBehaviour.print(e.Message);
                        }
                    }
                    //pass to location
                    else
                    {
                        GUI.DrawTexture(new Rect(Vector2.Scale(V3toV2(balls[i].FindPropertyRelative("location").vector3Value), size) + PlaceOnCourt(ballSize), ballSize), ballImage);
                    }
                    if (!balls[i].FindPropertyRelative("visible").boolValue)
                    {
                        balls.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    if (!balls[i].FindPropertyRelative("visible").boolValue)
                    {
                        balls.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        //drawing for defense
        else
        {
        }
        //GUI.DrawTexture(new Rect(Vector2.Scale(V3toV2(defensePassLocation.vector3Value), size) + PlaceOnCourt(ballSize), ballSize), ballImage);
        initial = false;
        initial = false;
        serializedObject.ApplyModifiedProperties();
        
        //GUI.Button(new Rect(new Vector2(10,y), buttonSize), buttonInfo);
    }

    void AddY(float additive = 20f)
    {
        y += additive;
    }

    Vector3 V2toV3(Vector2 temp)
    {
        temp = Vector2.Scale(temp, Vector2.one * 9);
        return new Vector3(temp.x, 1.1f, temp.y);
    }
    
    Vector2 V3toV2(Vector3 temp)
    {
        temp = Vector3.Scale(temp, Vector3.one * 1/9);
        return new Vector2(temp.x, temp.z);
    }

    Vector2 PlaceOnCourt(Vector2 imageSize)
    {
        return position + Vector2.right * size.x/2 - imageSize/2;
    }
}
