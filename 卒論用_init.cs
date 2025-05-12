using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.IO;
using System.Linq;
using NUnit.Framework.Internal.Builders;
using UnityEngine.SceneManagement;
using System;
using UnityEditor.SceneManagement;

public class grad_init : MonoBehaviour
{
    CharacterController controller;
    Animator animator;
    CinemachineFreeLook freeLook;
    private bool isCursorLocked = true;
    public GameObject freeLookCamera;
    float normalSpeed = 6f;
    float sprintSpeed = 10f;
    float jump = 16f;
    float gravity = 50f;
    Vector3 moveDirection = Vector3.zero;
    public GameObject targetObject;
    private List<string[]> csvData;
    private int frameIndex;

    // パラメータ
    [SerializeField]
    public bool ss1;
    [SerializeField]
    public bool ss2;
    [SerializeField]
    public bool ss3;
    [SerializeField]
    public bool ss4;
    [SerializeField]
    public bool ss5;
    [SerializeField]
    public bool ss6;
    [SerializeField]
    public bool ss7;
    [SerializeField]
    public bool ss8;
    [SerializeField]
    public bool ss9;
    [SerializeField]
    public bool ss10;
    [SerializeField, Range(0, 9)]
    public int ss1_pri;
    [SerializeField, Range(0, 9)]
    public int ss2_pri;
    [SerializeField, Range(0, 9)]
    public int ss3_pri;
    [SerializeField, Range(0, 9)]
    public int ss4_pri;
    [SerializeField, Range(0, 9)]
    public int ss5_pri;
    [SerializeField, Range(0, 9)]
    public int ss6_pri;
    [SerializeField, Range(0, 9)]
    public int ss7_pri;
    [SerializeField, Range(0, 9)]
    public int ss8_pri;
    [SerializeField, Range(0, 9)]
    public int ss9_pri;
    [SerializeField, Range(0, 9)]
    public int ss10_pri;

    // アイテムの位置
    Vector3 ss1_loc = new Vector3(170f, 48.97f, 303f);
    Vector3 ss2_loc = new Vector3(183f, 19.884f, 460f);
    Vector3 ss3_loc = new Vector3(352f, 52.008f, 285f);
    Vector3 ss4_loc = new Vector3(935f, 91.763f, 356f);
    Vector3 ss5_loc = new Vector3(544f, 19.208f, 19f);
    Vector3 ss6_loc = new Vector3(482f, 57.862f, 302f);
    Vector3 ss7_loc = new Vector3(759f, 52.687f, 471f);
    Vector3 ss8_loc = new Vector3(262f, 15.44f, 988f);
    Vector3 ss9_loc = new Vector3(844f, 71.148f, 660f);
    Vector3 ss10_loc = new Vector3(282.54f, 20.02f, 779.41f);

    // 戦略決定のための構造体・配列
    public struct ss
    {
        public int index;
        public bool on;
        public int pri;
        public Vector3 loc;

        // コンストラクタ
        public ss(int index, bool on, int pri, Vector3 loc)
        {
            this.index = index;
            this.on = on;
            this.pri = pri;
            this.loc = loc;
        }
    }
    ss[] ss_info;
    ss[] strategy;

    // オブジェクト回避用の変数
    Vector3 postPos = Vector3.zero;
    Vector3 nowPos = Vector3.zero;
    Vector3 fixed_nowPos = Vector3.zero;
    int avdCnt = 0;
    bool isAvding = false;
    bool isAvoidRight = false;

    // csvファイルパス
    string filePath = Path.Combine(Application.dataPath, "Logs", "init_fuzz.csv");

    void Start()
    {
        // バグ検出記録用
        try
        {
            // 指定したパスにファイルが存在しない場合は作成し、存在する場合は上書き
            using (StreamWriter writer = new StreamWriter(Path.Combine(Application.dataPath, "Logs", "detected_bugs.csv"), true))
            {
                writer.WriteLine("init");
            }
        }
        catch (IOException e)
        {
            Debug.LogError($"CSVファイルの作成に失敗しました: {e.Message}");
        }

        // パラメータ記録
        if (!File.Exists(Path.Combine(Application.dataPath, "Logs", "param_global.csv")))
        {
            try
            {
                // 指定したパスにファイルが存在しない場合は作成し、存在する場合は上書き
                using (StreamWriter writer = new StreamWriter(Path.Combine(Application.dataPath, "Logs", "param_global.csv"), false))
                {
                    // ヘッダー行を追加
                    writer.WriteLine("ss1,ss2,ss3,ss4,ss5,ss6,ss7,ss8,ss9,ss10,ss1_pri,ss2_pri,ss3_pri,ss4_pri,ss5_pri,ss6_pri,ss7_pri,ss8_pri,ss9_pri,ss10_pri");
                    writer.WriteLine($"{ss1},{ss2},{ss3},{ss4},{ss5},{ss6},{ss7},{ss8},{ss9},{ss10},{ss1_pri},{ss2_pri},{ss3_pri},{ss4_pri},{ss5_pri},{ss6_pri},{ss7_pri},{ss8_pri},{ss9_pri},{ss10_pri}");
                    Debug.Log($"パラメータを記録しました: param_global.csv");
                }
            }
            catch (IOException e)
            {
                Debug.LogError($"CSVファイルの作成に失敗しました: {e.Message}");
            }
        }

        // 今回のパラメータをcsvファイルから取得
        List<string> paramLines = new List<string>(File.ReadAllLines(Path.Combine(Application.dataPath, "Logs", "param_global.csv")));
        string paramLastLine = paramLines[paramLines.Count - 1];
        string[] paramColumns = paramLastLine.Split(',');
        Debug.Log("ss1: " + bool.Parse(paramColumns[0]));
        Debug.Log("ss10_pri: " + int.Parse(paramColumns[19]));

        // 構造体配列の初期化
        ss_info = new ss[10];
        ss_info[0] = new ss(1, bool.Parse(paramColumns[0]), int.Parse(paramColumns[10]), ss1_loc);
        ss_info[1] = new ss(2, bool.Parse(paramColumns[1]), int.Parse(paramColumns[11]), ss2_loc);
        ss_info[2] = new ss(3, bool.Parse(paramColumns[2]), int.Parse(paramColumns[12]), ss3_loc);
        ss_info[3] = new ss(4, bool.Parse(paramColumns[3]), int.Parse(paramColumns[13]), ss4_loc);
        ss_info[4] = new ss(5, bool.Parse(paramColumns[4]), int.Parse(paramColumns[14]), ss5_loc);
        ss_info[5] = new ss(6, bool.Parse(paramColumns[5]), int.Parse(paramColumns[15]), ss6_loc);
        ss_info[6] = new ss(7, bool.Parse(paramColumns[6]), int.Parse(paramColumns[16]), ss7_loc);
        ss_info[7] = new ss(8, bool.Parse(paramColumns[7]), int.Parse(paramColumns[17]), ss8_loc);
        ss_info[8] = new ss(9, bool.Parse(paramColumns[8]), int.Parse(paramColumns[18]), ss9_loc);
        ss_info[9] = new ss(10, bool.Parse(paramColumns[9]), int.Parse(paramColumns[19]), ss10_loc);

        // 配列をpriで降順にソートし、priが同値の場合はランダムな順序を維持
        System.Random random = new System.Random();
        strategy = ss_info.Where(i => i.on).OrderByDescending(i => i.pri).ThenBy(i => random.Next()).ToArray();

        // 戦略記録
        try
        {
            // 指定したパスにファイルが存在しない場合は作成し、存在する場合は上書き
            using (StreamWriter writer = new StreamWriter(Path.Combine(Application.dataPath, "Logs", "check_points.csv"), false))
            {
                writer.WriteLine("index,x,y,z"); // index0は，新しく追加するチェックポイント
            }
        }
        catch (IOException e)
        {
            Debug.LogError($"CSVファイルの作成に失敗しました: {e.Message}");
        }

        // ソート後の配列を出力
        Debug.Log("=== ソート後の配列 ===");
        foreach (var i in strategy)
        {
            Debug.Log($"Index: {i.index}, Priority: {i.pri}, Location: {i.loc}");
            // 戦略記録
            try
            {
                // 指定したパスにファイルが存在しない場合は作成し、存在する場合は上書き
                using (StreamWriter writer = new StreamWriter(Path.Combine(Application.dataPath, "Logs", "check_points.csv"), true))
                {
                    writer.WriteLine($"{i.index},{i.loc.x},{i.loc.y},{i.loc.z}");
                }
            }
            catch (IOException e)
            {
                Debug.LogError($"CSVファイルの作成に失敗しました: {e.Message}");
            }
        }

        // バグを非アクティブ化
        GameObject stuckBugsObject = GameObject.Find("StuckBugs");
        if (stuckBugsObject != null)
        {
            stuckBugsObject.SetActive(false);
            Debug.Log("StuckBugs オブジェクトを非アクティブにしました。");
        }
        else
        {
            Debug.LogError("StuckBugs オブジェクトが見つかりませんでした。");
        }

        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        freeLookCamera = GameObject.Find("FreeLook Camera");
        freeLook = freeLookCamera.GetComponent<CinemachineFreeLook>();
        LockCursor();

        frameIndex = 0;

        // csvファイル作成
        try
        {
            // 指定したパスにファイルが存在しない場合は作成し、存在する場合は上書き
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                // ヘッダー行を追加
                writer.WriteLine("speed,v,h,forwardX,forwardZ,jump,item");
                Debug.Log($"CSVファイル 'init_fuzz.csv' を作成しました: {filePath}");
            }
        }
        catch (IOException e)
        {
            Debug.LogError($"CSVファイルの作成に失敗しました: {e.Message}");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleCursorLock();
        }

        animator.SetBool("LeftClick", false);

        // タイムアウト
        if (frameIndex == 72000)
        {
            ss1_pri++;
            Debug.Log("タイムアウト");
            // 次に局所ファジングを実行することを記述
            if (File.Exists(Path.Combine(Application.dataPath, "Logs", "fuzz_type.csv")))
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(Application.dataPath, "Logs", "fuzz_type.csv"), true))
                {
                    writer.WriteLine("init");
                }
                Debug.Log("fuzz_type.csv ファイルに書き込みました。");
            }
            else
            {
                Debug.Log("エラー: fuzz_type.csv ファイルは存在しません。");
                UnityEditor.EditorApplication.isPlaying = false;
            }
            MutateParam();
            Scene scene = SceneManager.GetActiveScene(); // 現在のシーンを取得
            SceneManager.LoadScene(scene.path); // シーンを再読み込み
            return;
        }

        //// アイテムに到達するとstrategyから消す
        //if (nowPos.x == strategy[0].loc.x && nowPos.z == strategy[0].loc.z)
        //{
        //    strategy = strategy.Skip(1).ToArray();
        //}
        // アイテムに到達するとstrategyから消す
        if (Mathf.Abs(nowPos.x - strategy[0].loc.x) <= 0.1f && Mathf.Abs(nowPos.z - strategy[0].loc.z) <= 0.1f)
        {
            // 全ての行を読み込み
            List<string> lines = new List<string>(File.ReadAllLines(filePath));
            int lastLine = lines.Count - 1;
            // 行の末尾に取得したアイテム名を追加
            lines[lastLine] += strategy[0].index.ToString();
            // 上書き保存
            File.WriteAllLines(filePath, lines.ToArray());

            strategy = strategy.Skip(1).ToArray();

            if (strategy.Length == 0)
            {
                ss1_pri++;
                Debug.Log("初期入力生成完了");
                // 次に局所ファジングを実行することを記述
                if (File.Exists(Path.Combine(Application.dataPath, "Logs", "fuzz_type.csv")))
                {
                    using (StreamWriter writer = new StreamWriter(Path.Combine(Application.dataPath, "Logs", "fuzz_type.csv"), true))
                    {
                        writer.WriteLine("local");
                    }
                    Debug.Log("fuzz_type.csv ファイルに書き込みました。");
                }
                else
                {
                    Debug.Log("エラー: fuzz_type.csv ファイルは存在しません。");
                    UnityEditor.EditorApplication.isPlaying = false;
                }
                MutateParam();
                Scene scene = SceneManager.GetActiveScene(); // 現在のシーンを取得
                SceneManager.LoadScene(scene.path); // シーンを再読み込み
                return;
            }
            Debug.Log("アイテム収集");
        }

        // 障害物に引っかかっているかどうかの判定
        if (frameIndex % 60 == 0)
        {
            float distance = Vector3.Distance(new Vector3(postPos.x, 0, postPos.z), new Vector3(nowPos.x, 0, nowPos.z));
            if(distance < 1f)
            {
                isAvding = true;
            }
            postPos = nowPos;
        }

        Vector3 moveZ;
        Vector3 moveX;
        Vector3 direction;
        if (isAvding == false)
        {
            // アイテムの方向に進む
            direction = new Vector3(strategy[0].loc.x - nowPos.x, 0f, strategy[0].loc.z - nowPos.z).normalized;
            moveZ = direction * 1 * sprintSpeed;
            moveX = new Vector3(0f, 0f, 0f);
        }
        else
        {
            if(avdCnt == 0) // 後ろに下がり始める
            {
                Vector3 forwardDirection = transform.forward;
                direction = new Vector3(-transform.forward.x, transform.forward.y, -transform.forward.z);
                moveZ = direction * 1 * sprintSpeed;
                moveX = new Vector3(0f, 0f, 0f);
                avdCnt++;
            }
            else if (avdCnt < 60) // 後ろに下がり続ける
            {
                direction = transform.forward;
                moveZ = direction * 1 * sprintSpeed;
                moveX = new Vector3(0f, 0f, 0f);
                avdCnt++;
            }
            else if(avdCnt == 60) //左右に曲がり始める
            {
                int i = UnityEngine.Random.Range(0, 2);
                if (i == 0) // 左方向のベクトル
                {
                    isAvoidRight = false;
                    direction = -transform.right;
                }
                else // 右方向のベクトル
                {
                    isAvoidRight = true;
                    direction = transform.right;
                }
                moveZ = direction * 1 * sprintSpeed;
                moveX = new Vector3(0f, 0f, 0f);
                avdCnt++;
            }
            else if (avdCnt < 120) // 左右に曲がり続ける
            {
                direction = transform.forward;
                moveZ = direction * 1 * sprintSpeed;
                moveX = new Vector3(0f, 0f, 0f);
                avdCnt++;
            }
            else if (avdCnt == 120) // 前進し始める
            {
                if (!isAvoidRight)
                {
                    direction = -transform.right;
                }
                else
                {
                    direction = transform.right;
                }
                moveZ = direction * 1 * sprintSpeed;
                moveX = new Vector3(0f, 0f, 0f);
                avdCnt++;
            }
            else if (avdCnt < 239)
            {
                direction = transform.forward;
                moveZ = direction * 1 * sprintSpeed;
                moveX = new Vector3(0f, 0f, 0f);
                avdCnt++;
            }
            else // avdCnt == 239
            {
                direction = transform.forward;
                moveZ = direction * 1 * sprintSpeed;
                moveX = new Vector3(0f, 0f, 0f);
                avdCnt = 0;
                isAvding = false;
            }
        }

        // csvファイルに保存
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                string newLine = $"10,1,0,{direction.x},{direction.z},0,";
                writer.WriteLine(newLine);
                //Debug.Log($"CSVファイルに以下の行を追加しました: {newLine}");
            }
        }
        catch (IOException e)
        {
            Debug.LogError($"CSVファイルの書き込みに失敗しました: {e.Message}");
        }

        if (controller.isGrounded)
        {
            moveDirection = moveZ + moveX;
        }
        else
        {
            moveDirection += new Vector3(0, -gravity * Time.deltaTime, 0);
        }
        animator.SetFloat("MoveSpeed", (moveZ + moveX).magnitude);
        controller.Move(moveDirection * Time.deltaTime);
        nowPos = transform.position;
        //Debug.Log(nowPos);
        transform.LookAt(nowPos + moveZ + moveX);
        frameIndex++;
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        SetFreeLookSpeed(900, 6);
        isCursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SetFreeLookSpeed(0, 0);
        isCursorLocked = false;
    }

    private void ToggleCursorLock()
    {
        if (isCursorLocked)
            UnlockCursor();
        else
            LockCursor();
    }

    private void SetFreeLookSpeed(float xSpeed, float ySpeed)
    {
        freeLook.m_XAxis.m_MaxSpeed = xSpeed;
        freeLook.m_YAxis.m_MaxSpeed = ySpeed;
    }

    // ペルソナ: 全てのアイテムを回収
    private void MutateParam()
    {
        int minNum = 10; // 以上
        int maxNum = 11; // 未満
        // 要素数10の配列を作成
        bool[] boolArray = new bool[10];
        // 全要素をFalseで初期化
        for (int i = 0; i < boolArray.Length; i++)
        {
            boolArray[i] = false;
        }

        // 全てのインデックスにTrueを設定
        int trueCount = 0;
        while (trueCount < UnityEngine.Random.Range(minNum, maxNum))
        {
            int randomIndex = UnityEngine.Random.Range(0, boolArray.Length);
            if (!boolArray[randomIndex])  // まだTrueが設定されていない場所にのみTrueを設定
            {
                boolArray[randomIndex] = true;
                trueCount++;
            }
        }

        List<string> csvLines = new List<string>(File.ReadAllLines(Path.Combine(Application.dataPath, "Logs", "param_global.csv")));
        Debug.Log("次は局所ファジングなので，localパラメータを変異します．");
        csvLines.Add($"{boolArray[0]},{boolArray[1]},{boolArray[2]},{boolArray[3]},{boolArray[4]},{boolArray[5]},{boolArray[6]},{boolArray[7]},{boolArray[8]},{boolArray[9]}," +
            $"{UnityEngine.Random.Range(0, 10)},{UnityEngine.Random.Range(0, 10)},{UnityEngine.Random.Range(0, 10)},{UnityEngine.Random.Range(0, 10)},{UnityEngine.Random.Range(0, 10)},{UnityEngine.Random.Range(0, 10)},{UnityEngine.Random.Range(0, 10)},{UnityEngine.Random.Range(0, 10)},{UnityEngine.Random.Range(0, 10)},{UnityEngine.Random.Range(0, 10)}");
        // ファイルに書き戻す
        File.WriteAllLines(Path.Combine(Application.dataPath, "Logs", "param_global.csv"), csvLines.ToArray());
    }

    //// それぞれのアイテムは8割の確率で収集される．アイテムは番号が若い方が優先されやすい．
    //private void MutateParam()
    //{
    //    // Trueになる確率
    //    float trueProb = 0.8f;
    //    // 要素数10の配列を作成
    //    bool[] boolArray = new bool[10];
    //    // True or Falseを配列に格納
    //    for (int i = 0; i < boolArray.Length; i++)
    //    {
    //        boolArray[i] = UnityEngine.Random.value < trueProb;
    //    }

    //    int Random0to9(int input)
    //    {
    //        // 引数が0から9の範囲外なら例外を投げる
    //        if (input < 0 || input > 9)
    //        {
    //            throw new ArgumentOutOfRangeException("input", "Input must be between 0 and 9");
    //        }

    //        // 9 - input を中心とする重み付きリストを作成
    //        int centerValue = 9 - input;
    //        System.Collections.Generic.List<int> weightedList = new System.Collections.Generic.List<int>();

    //        for (int i = 0; i < 10; i++)
    //        {
    //            int weight = Mathf.Max(10 - Math.Abs(centerValue - i), 1); // 中心に近いほど重みが大きい
    //            for (int j = 0; j < weight; j++)
    //            {
    //                weightedList.Add(i);
    //            }
    //        }
    //        //Debug.Log("All Numbers: " + string.Join(", ", weightedList));
    //        // ランダムで値を選択
    //        int randomIndex = UnityEngine.Random.Range(0, weightedList.Count);
    //        return weightedList[randomIndex];
    //    }
    //    List<string> csvLines = new List<string>(File.ReadAllLines(Path.Combine(Application.dataPath, "Logs", "param_global.csv")));
    //    Debug.Log("次は局所ファジングなので，localパラメータを変異します．");
    //    csvLines.Add($"{boolArray[0]},{boolArray[1]},{boolArray[2]},{boolArray[3]},{boolArray[4]},{boolArray[5]},{boolArray[6]},{boolArray[7]},{boolArray[8]},{boolArray[9]}," +
    //        $"{Random0to9(0)},{Random0to9(1)},{Random0to9(2)},{Random0to9(3)},{Random0to9(4)},{Random0to9(5)},{Random0to9(6)},{Random0to9(7)},{Random0to9(8)},{Random0to9(9)}");
    //    // ファイルに書き戻す
    //    File.WriteAllLines(Path.Combine(Application.dataPath, "Logs", "param_global.csv"), csvLines.ToArray());
    //}
}
