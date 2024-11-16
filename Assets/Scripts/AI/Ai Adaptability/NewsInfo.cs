using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Article
{
    public Source source;
    public string author;
    public string title;
    public string description;
    public string url;
    public string urlToImage;
    public string publishedAt;
    public string content;
}

[Serializable]
public class Source
{
    public string id;
    public string name;
}

[Serializable]
public class NewsResponse
{
    public string status;
    public int totalResults;
    public List<Article> articles;
}


public class NewsInfo: AIModule
{
    public List<Article> returnArticle;


    // public void getnewsyay(string intrest, string pagesize)
    // {
    //     StartCoroutine(GetNewsAsync(intrest, "2024-10-10", "2024-11-06", "popularity", pagesize));      
    // }

    // protected override void Start()
    // {
    //     getnewsyay();
    // }

    public IEnumerator GetNewsAsync(string intrest, string from, string to, string sortby, string pagesize)
    {   
        var apiUrl = "https://newsapi.org/v2/everything?";
        apiUrl += "q=" + intrest + "&";
        apiUrl += "from=" + from + "&";
        apiUrl += "to=" + to + "&";
        apiUrl += "sortBy=" + sortby + "&";
        apiUrl += "pageSize=" + pagesize + "&";
        apiUrl += "apiKey=" + _brain.NewsAPIKey.ToString();

        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var JSONResponse = request.downloadHandler.text;
                NewsResponse newsResponse = JsonUtility.FromJson<NewsResponse>(JSONResponse);

                // Debug.Log(newsResponse.status);
                // Debug.Log(newsResponse.totalResults);

                foreach(var article in newsResponse.articles)
                {
                    // Debug.Log(article.source.name);
                    // Debug.Log(article.title);
                    if (article.description != "[Removed]") 
                    {
                        // Debug.Log(article.description);
                        returnArticle.Add(article);
                    }
                }

                // Debug.Log(returnArticle);
            }
            else{
                Debug.LogError(request.error);
            }
        }
    }
}   