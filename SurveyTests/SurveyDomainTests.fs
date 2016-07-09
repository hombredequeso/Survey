namespace SurveyTests

open NUnit.Framework
open System
open FsUnit
open System
open Survey.SurveyDomain
open Hombredequeso.Rop

[<TestFixture>]
module SurveyDomainTests =

    type QuestionRop = RopResult<Question.T, Question.Error>
    type AddNewAnswerRop = RopResult<Answer list, AddNewAnswerError>
    
    let take n l = l 
                    |> List.mapi (fun i x -> (i,x)) 
                    |> List.filter (fun (i,x) -> i < n)
                    |> List.map (fun (i,x) -> x)

    let idList = [
        Guid.Parse("00000000-C05A-418A-A320-19B3F770A3B1");
        Guid.Parse("00000001-C05A-418A-A320-19B3F770A3B1");
        Guid.Parse("00000002-C05A-418A-A320-19B3F770A3B1");
        Guid.Parse("00000003-C05A-418A-A320-19B3F770A3B1");
        Guid.Parse("00000004-C05A-418A-A320-19B3F770A3B1");
        Guid.Parse("00000005-C05A-418A-A320-19B3F770A3B1");
        Guid.Parse("00000006-C05A-418A-A320-19B3F770A3B1");
        Guid.Parse("00000007-C05A-418A-A320-19B3F770A3B1");
    ]

    let sampleSurvey: Survey = {
        id = Guid.NewGuid()
        Sections = [
                        {
                            Questions = [
                                            {
                                                id = idList.[0]
                                                question = Question.Question "question0"
                                            };
                                            {
                                                id = idList.[1]
                                                question = Question.Question "question1"
                                            };
                                            {
                                                id = idList.[2]
                                                question = Question.Question "question2"
                                            };
                                        ]
                        };
                        {
                            Questions = [
                                            {
                                                id = idList.[3]
                                                question = Question.Question "question3"
                                            };
                                            {
                                                id = idList.[4]
                                                question = Question.Question "question4"
                                            };
                                        ]
                        };
                        {
                            Questions = [
                                            {
                                                id = idList.[5]
                                                question = Question.Question "question5"
                                            };
                                        ]
                        };
                        {
                            Questions = [
                                            {
                                                id = idList.[6]
                                                question = Question.Question "question6"
                                            };
                                            {
                                                id = idList.[7]
                                                question = Question.Question "question7"
                                            };
                                        ]
                        };
        ]
    }

    [<Test>]
    let ``Valid question returns a succeed`` () =
        let q = "Why did the chicken cross the road?"
        let question: QuestionRop = Question.create q
        let expectedQuestion:QuestionRop = (Question.Question q |> succeed)
        question |> should equal expectedQuestion

    [<Test>]
    let ``InValid question too short returns a fail`` () =
        let q = "Why"
        let question: QuestionRop = Question.create q
        let expectedResult: QuestionRop = Question.Error.TooShort |> fail
        question |> should equal expectedResult

    let createRandomSurveyQuestion id x =
        let q = Question.Question ("abc" + x.ToString() + "." + id.ToString())
        {
            id = id
            question = q
        }

    [<Test>]
    let ``getNext returns ErrorNoSurveyQuestions when there are no survey questions`` () =
        let emptySurvey: Survey = {
            id = Guid.NewGuid()
            Sections = []
        }
        let nextQuestion = getNext emptySurvey []
        let expectedResult:RopResult<SurveyQuestion, NoNextResult> = ErrorNoSurveyQuestions |> fail
        nextQuestion |> should equal expectedResult

    [<Test>]
    let ``getNext returns first question where no Answers`` () =
        let nextQuestion :RopResult<SurveyQuestion, NoNextResult> = 
                getNext sampleSurvey []
        let expectedResult:RopResult<SurveyQuestion, NoNextResult> = 
                sampleSurvey.Sections.[0].Questions.[0] |> succeed
        nextQuestion |> should equal expectedResult

    [<Test>]
    let ``getNext returns SurveyComplete if all questions answered`` () =
        let allAnswers = idList|> List.map (fun x -> {
                                                        questionId=x
                                                        result=true})

        let nextQuestion = getNext sampleSurvey allAnswers

        match nextQuestion with
            | Failure [SurveyComplete]  -> Assert.Pass()
            | _ -> Assert.Fail()

    [<Test>]
    let ``getNext returns next question after a true answer`` () =

        let firstAnswerOnly = idList |> take 1 |> List.map (fun x -> {
                                                                        questionId=x
                                                                        result=true})

        let nextQuestion = getNext sampleSurvey firstAnswerOnly

        match nextQuestion with
            | Success (secondQuestion, _)  -> secondQuestion |> should equal sampleSurvey.Sections.[0].Questions.[1]
            | _ -> Assert.Fail()

    [<Test>]
    let ``getNext returns next section first question after a false answer`` () =
        let firstAnswerOnly = idList |> take 1 |> List.map (fun x -> {
                                                                        questionId=x
                                                                        result=false})

        let nextQuestion = getNext sampleSurvey firstAnswerOnly

        match nextQuestion with
            | Success (secondSectionFirstQuestion, _)  -> 
                secondSectionFirstQuestion 
                    |> should equal sampleSurvey.Sections.[1].Questions.[0]
            | _ -> Assert.Fail()



    [<Test>]
    let ``addNewAnswer returns newAnswer added onto list`` () =
        let answers = []
        let survey = sampleSurvey
        let newAnswer = {
           questionId = sampleSurvey.Sections.[0].Questions.[0].id
           result = true
        }

        let result : AddNewAnswerRop = addNewAnswer survey answers newAnswer
        let expectedResult: AddNewAnswerRop = [newAnswer] |> succeed


        result |> should equal expectedResult

    [<Test>]
    let ``addNewAnswer returns Fail when newAnswer is not in survey`` () =
        let answers = []
        let survey = sampleSurvey
        let newAnswer = {
           questionId = Guid.NewGuid()
           result = true
        }

        let result: AddNewAnswerRop = addNewAnswer survey answers newAnswer
        let expectedResult : AddNewAnswerRop = AnswerNotForSurvey |> fail
        result |> should equal expectedResult


    [<Test>]
    let ``addNewAnswer returns Success when newAnswer is next question`` () =
        let firstAnswerOnly = idList |> take 1 |> List.map (fun x -> {
                                                                        questionId=x
                                                                        result=false})
        let nextQuestion = sampleSurvey.Sections.[1].Questions.[0]

        let newAnswer = {
            questionId = nextQuestion.id
            result = true
        }

        let result: AddNewAnswerRop = addNewAnswer sampleSurvey firstAnswerOnly newAnswer
        let expectedResult:AddNewAnswerRop = newAnswer::firstAnswerOnly |> succeed
        result |> should equal expectedResult


    [<Test>]
    let ``addNewAnswer returns Fail when newAnswer is not next question`` () =
        let firstAnswerOnly = idList |> take 1 |> List.map (fun x -> {
                                                                        questionId=x
                                                                        result=false})
        let nextQuestion = sampleSurvey.Sections.[1].Questions.[1]

        let newAnswer = {
            questionId = nextQuestion.id
            result = true
        }

        let result: AddNewAnswerRop = addNewAnswer sampleSurvey firstAnswerOnly newAnswer
        let expectedResult:AddNewAnswerRop = AnswerNotForNextQuestion |> fail
        result |> should equal expectedResult



        // can answer next question after a yes
        // can answer first question of next section after a no
        // can update latest answer in any question
