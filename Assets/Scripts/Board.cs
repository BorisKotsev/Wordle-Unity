using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Board : MonoBehaviour
{
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[] 
    {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z,
    };

    public Tile.State emptyState;
    public Tile.State occupiedState;
    public Tile.State correctState;
    public Tile.State wrondSpotState;
    public Tile.State incorrectState;

    public TextMeshProUGUI invalidWordText;
    public TextMeshProUGUI giveUpText;
    public Button newWordBtn;
    public Button giveUpBtn;

    private int rowIndex, colIndex;

    private Row[] rows;

    private string[] solutions;
    private string[] validWords;
    private string word;

    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
    }

    private void Start()
    {
        loadData();
        newGame();
    }

    public void newGame()
    {
        clearBoard();
        setRandomWord();
        
        enabled = true;
    } 

    public void giveUp()
    {
        if(rowIndex < rows.Length)
        {
            giveUpText.text = "The word is ";
            giveUpText.gameObject.SetActive(true);
            giveUpText.text += word;
            
            enabled = true;

            newWordBtn.gameObject.SetActive(true);
        }
        else
        {
            giveUpText.text = "The word is ";
            giveUpText.gameObject.SetActive(true);
            giveUpText.text += word;
        }
    }

    private void loadData()
    {
        TextAsset textFile = Resources.Load("official_wordle_all") as TextAsset;
        validWords = textFile.text.Split('\n');

        textFile = Resources.Load("official_wordle_common") as TextAsset;
        solutions = textFile.text.Split('\n');
    } 

    private void setRandomWord()
    {
        word = solutions[Random.Range(0, solutions.Length)];
        word = word.ToLower().Trim();
    }

    private void Update()
    {
        Row currRow = rows[rowIndex];

        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            colIndex = Mathf.Max(colIndex - 1, 0);
            
            currRow.tiles[colIndex].SetLetter('\0');
            currRow.tiles[colIndex].setState(emptyState);

            invalidWordText.gameObject.SetActive(false);
        }
        else if(colIndex >= currRow.tiles.Length)
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                submitRow(currRow);
            }
        }
        else
        {
            for (int i = 0; i < SUPPORTED_KEYS.Length; i++)
            {
                if (Input.GetKeyDown(SUPPORTED_KEYS[i]))
                {
                    currRow.tiles[colIndex].SetLetter((char)SUPPORTED_KEYS[i]);
                    currRow.tiles[colIndex].setState(occupiedState);
                    colIndex++;
                    break;
                }
            }
        }
    } 

    private void submitRow(Row row)
    {
        giveUpBtn.gameObject.SetActive(true);

        if(!isValidWord(row.word))
        {
            invalidWordText.gameObject.SetActive(true);

            return;
        }

        string remaining = word;

        for(int i = 0; i < row.tiles.Length; i ++)
        {
            Tile tile = row.tiles[i];

            if(tile.letter == word[i])
            {
                tile.setState(correctState);

                remaining = remaining.Remove(i, 1);
                remaining = remaining.Insert(i, " ");
            }
            else if (!word.Contains(tile.letter))
            {
                tile.setState(incorrectState);
            }
        }

        for(int i = 0; i < row.tiles.Length; i ++)
        {
            Tile tile = row.tiles[i];

            if(tile.state != correctState && tile.state != incorrectState)
            {
                if(remaining.Contains(tile.letter))
                {
                    tile.setState(wrondSpotState);

                    int index = remaining.IndexOf(tile.letter);

                    remaining = remaining.Remove(index, 1);
                    remaining = remaining.Insert(index, " ");
                }
                else
                {
                    tile.setState(incorrectState);
                }
            } 
        }

        if(hasWon(row))
        {
            enabled = false;
        }

        rowIndex ++; 
        colIndex = 0;

        if(rowIndex >= rows.Length)
        {
            enabled = false;
        }
    }

    private void clearBoard()
    {
        giveUpText.gameObject.SetActive(false);

        for(int i = 0; i < rows.Length; i ++)
        {
            for(int j = 0; j < rows[i].tiles.Length; j ++)
            {
                rows[i].tiles[j].SetLetter('\0');
                rows[i].tiles[j].setState(emptyState);
            }
        }

        rowIndex = 0; 
        colIndex = 0;
    }

    private bool isValidWord(string word)
    {
        for(int i = 0; i < validWords.Length; i ++)
        {
            if(word == validWords[i])
            {
                return true;
            }
        }

        return false;
    }

    private bool hasWon(Row row)
    {
        for(int i = 0; i < row.tiles.Length; i ++)
        {
            if(row.tiles[i].state != correctState)
            {
                return false;
            }
        }

        return true;
    }

    private void OnEnable()
    {
        newWordBtn.gameObject.SetActive(false);
        giveUpBtn.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        newWordBtn.gameObject.SetActive(true);
        giveUpBtn.gameObject.SetActive(true);
    }
}
