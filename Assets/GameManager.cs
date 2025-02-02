using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Mono.Cecil.Cil;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class QuestionData
{
    public string indonesiaKata;
    public string answerInggris;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private ScriptableAnswer questionDataScriptable;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private WordData[] AnswerPrefabs;
    [SerializeField] private WordData[] WordPrefabs;
    [SerializeField] private GameObject gameOverWins;
    [SerializeField] private GameObject gameOverLose;
    [SerializeField] private Player player, enemy;
    private char[] charWordArray = new char[12];
    private int currentAnswerIndex = 0;
    private bool isAnswer = true;
    private List<int> WordSelectIndex;
    private int currentQuestionIndex = 0;
    private GameState gameState = GameState.OnPLay;
    private string answerWords;

    public enum GameState
    {
        OnPLay,
        Next
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        WordSelectIndex = new List<int>();

    }

    private void Start()
    {
        WordSelectIndex = new List<int>();
        QuestionSet();
    }

    private void Update()
    {
        player.UpdateHealth();
        enemy.UpdateHealth();
    }



    private void QuestionSet()
    {
        gameState = GameState.OnPLay;

        questionText.text = questionDataScriptable.questions[currentQuestionIndex].indonesiaKata;
        answerWords = questionDataScriptable.questions[currentQuestionIndex].answerInggris;

        QuestionResets();
        WordSelectIndex.Clear();

        Array.Clear(charWordArray, 0, charWordArray.Length);

        for (int i = 0; i < answerWords.Length; i++)
        {
            charWordArray[i] = char.ToUpper(answerWords[i]);
        }

        for (int i = answerWords.Length; i < WordPrefabs.Length; i++)
        {
            charWordArray[i] = (char)UnityEngine.Random.Range(65, 91);
        }

        charWordArray = ShuffleWordList.ShuffleListItems<char>(charWordArray.ToList()).ToArray();

        for (int i = 0; i < WordPrefabs.Length; i++)
        {
            WordPrefabs[i].SetChar(charWordArray[i]);
        }


    }

    public void SelectedOptions(WordData wordData)
    {

        if (gameState == GameState.Next || currentAnswerIndex >= answerWords.Length)
        {
            return;
        }

        WordSelectIndex.Add(wordData.transform.GetSiblingIndex());
        wordData.gameObject.SetActive(false);
        AnswerPrefabs[currentAnswerIndex].SetChar(wordData.CharValue);

        currentAnswerIndex++;
        if (player.health <= 0)
        {
            gameOverLose.SetActive(true);
        }

        if (currentAnswerIndex == answerWords.Length)
        {
            isAnswer = true;

            for (int i = 0; i < answerWords.Length; i++)
            {
                if (char.ToUpper(answerWords[i]) != char.ToUpper(AnswerPrefabs[i].CharValue))
                {
                    isAnswer = false;
                    break;
                }
            }


            if (isAnswer)
            {
                player.health += player.restoreHealth;
                Debug.Log("Jawaban Benar");
                gameState = GameState.Next;
                currentQuestionIndex++;

                if (currentQuestionIndex < questionDataScriptable.questions.Count)
                {
                    Invoke("QuestionSet", 0.5f);
                }
                else
                {
                    Debug.Log("Game Selesai");
                    gameOverWins.SetActive(true);
                    enemy.health = 0;
                    player.AnimateAttack();
                }
            }
            else if (!isAnswer)
            {
                enemy.health += 5;
                Debug.Log("Salah");
                player.TakeDamage();
                if (player.health <= 0)
                {
                    gameOverLose.SetActive(true);
                    player.health = 0;
                    enemy.AnimateAttack();
                }
                else
                {
                    QuestionResets();
                }
            }
        }
    }


    public void QuestionResets()
    {
        for (int i = 0; i < AnswerPrefabs.Length; i++)
        {
            AnswerPrefabs[i].gameObject.SetActive(true);
            AnswerPrefabs[i].SetChar('_');
        }
        for (int i = answerWords.Length; i < AnswerPrefabs.Length; i++)
        {
            AnswerPrefabs[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < WordPrefabs.Length; i++)
        {
            WordPrefabs[i].gameObject.SetActive(true);
        }
        currentAnswerIndex = 0;
    }

    public void hintAnswer()
    {

        if (AnswerPrefabs[currentAnswerIndex].CharValue.Equals('_'))
        {
            AnswerPrefabs[currentAnswerIndex].SetChar(char.ToUpper(answerWords[currentAnswerIndex]));
            currentAnswerIndex++;
        }

        player.health = player.health - player.attackDamage;
        if (player.health <= 0)
        {
            player.health = 0;
            gameOverLose.SetActive(true);
        }
        if (currentAnswerIndex == answerWords.Length)
        {
            isAnswer = true;
            for (int i = 0; i < answerWords.Length; i++)
            {
                if (char.ToUpper(answerWords[i]) != char.ToUpper(AnswerPrefabs[i].CharValue))
                {
                    isAnswer = false;
                    break;
                }
            }

            if (isAnswer)
            {
                Debug.Log("Jawaban Benar");
                gameState = GameState.Next;
                currentQuestionIndex++;

                if (currentQuestionIndex < questionDataScriptable.questions.Count)
                {
                    Invoke("QuestionSet", 0.5f);
                }
                else
                {
                    Debug.Log("Game Selesai");
                    gameOverWins.SetActive(true);
                }
            }
            else if (!isAnswer)
            {
                Debug.Log("Salah");
                player.TakeDamage();
                if (player.health <= 0)
                {
                    gameOverLose.SetActive(true);

                }
                else
                {
                    QuestionResets();
                }
            }
        }

    }


    public void LastWordReset()
    {
        try
        {
            if (WordSelectIndex.Count > 0)
            {
                int index = WordSelectIndex[WordSelectIndex.Count - 1];
                WordPrefabs[index].gameObject.SetActive(true);
                WordSelectIndex.RemoveAt(WordSelectIndex.Count - 1);

                currentAnswerIndex--;
                AnswerPrefabs[currentAnswerIndex].SetChar('_');
            }
        }
        catch (IndexOutOfRangeException index)
        {
            QuestionResets();
            Debug.Log("index out of range" + index.Message);
        }
    }

}

