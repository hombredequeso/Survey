namespace SurveyTests

open NUnit.Framework
open System
open FsUnit
open System
open Survey
open Hombredequeso.Rop
open Nancy.Testing
open Nancy
open Newtonsoft.Json

[<TestFixture>]
module SurveyModuleTests =

    [<Test>]
    let ``Simple Test`` () =
        let bootstrapper = new DefaultNancyBootstrapper()
        let browser = new Browser(bootstrapper)
        let config (w:BrowserContext) =
            w.HttpRequest()
        let result = browser.Get("/api/survey/helloworld", config)
        result.StatusCode |> should equal HttpStatusCode.OK
        let s = result.Body.AsString()
        Console.WriteLine s
        result.Body |> should equal "hello back at ya"

    [<Test>]
    let ``Post Body Deserialize test that fails`` () =
        let bootstrapper = new DefaultNancyBootstrapper()
        let browser = new Browser(bootstrapper)
        let postBody = "{\"answer\":\"trueish\"}"
        let config (w:BrowserContext) =
            w.HttpRequest()
            w.Body(postBody)
            w.Header("content-type", "application/json");
        let result = browser.Post("/api/survey/helloworld", config)
        let s = result.Body.AsString()
        Console.WriteLine s
        result.StatusCode |> should equal HttpStatusCode.BadRequest

    [<Test>]
    let ``Post Answer returns OK, Answer`` () =
        let bootstrapper = new DefaultNancyBootstrapper()
        let browser = new Browser(bootstrapper)
        let postBody = "{\"answer\":\"true\"}"
        let surveyId = Guid.NewGuid()
        let questionId = SurveyDal.sampleSurvey.Sections.[0].Questions.[0].id

        let config (w:BrowserContext) =
            w.HttpRequest()
            w.Body(postBody)
            w.Header("content-type", "application/json");
        let url = sprintf "/api/survey/%A/question/%A/answer" surveyId questionId
        let response = browser.Post(url, config)
        let s = response.Body.AsString()
        let answerResponse = JsonConvert.DeserializeObject<SirenEntity<Answer>>(s)

        let expectedAnswerResponse: SirenEntity<Answer> = 
            {
                klass = [|"answer"|]
                properties = {
                                surveyId = surveyId
                                questionId = questionId
                                answer = true
                             }
                links = [||]
            }

        response.StatusCode |> should equal HttpStatusCode.Created
        Console.WriteLine s
        answerResponse |> should equal expectedAnswerResponse