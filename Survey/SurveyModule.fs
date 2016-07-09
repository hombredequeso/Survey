namespace Survey

open System
open Nancy
open Nancy.ModelBinding 
open Nancy.Extensions
open SurveyDomain
open Hombredequeso.Rop
open Hombredequeso.TryParser

type Question = {
    id: Guid
    section: int
    questionNumber: int
    text: string
}

[<CLIMutable>]
type PostAnswerBody = {
    answer: bool
}

[<CLIMutable>]
type Answer = {
    surveyId: Guid
    questionId: Guid
    answer: bool
}

type SurveyStatus = {
    id: Guid
    status: string
}

type Error = {
    messages: string[] 
}

type Link = {
    rel: string[]
    href: string
}

type SirenEntity<'T> = {
    klass: string[]
    properties: 'T
    links: Link[]
}

type GetNextQuestionError =
    | InvalidSurveyId
    | SurveyDoesNotExist of Guid
    | NoNextResult of NoNextResult * Guid

type PostAnswerError =
    | InvalidSurveyIdP
    | InvalidQuestionIdP
    | InvalidBodyP
    | SurveyDoesNotExistP of Guid
    | AddNewAnswerError of AddNewAnswerError

type SurveyModule() as x =
    inherit NancyModule()

    let getSurveyStatus id status = {id = id; status = status}
    let getSurveyStatusResponse (surveyStatus: SurveyStatus) =
        {
            klass = [|"surveyStatus"|]
            properties = surveyStatus
            links = [||]
        }
    let getError msg = {messages = [|msg|]}

    let getParamValue (dynamicdictionary: obj) value = 
        let parameters = dynamicdictionary :?> Nancy.DynamicDictionary
        let paramValue = parameters.[value] :?> Nancy.DynamicDictionaryValue
        match paramValue.HasValue with
            | false -> None
            | true -> paramValue.ToString() |> Some

    let toSurveyResponse (s: SurveyQuestion) =
        {
            klass = [|"question"|]
            properties = {
                            id = s.id
                            section = 0
                            questionNumber = 0
                            text = Question.value s.question
                         }
            links = [||]
        }

    let toAnswerResponse ((surveyId, questionId, answer):Guid*Guid*bool) =
        {
            klass = [|"answer"|]
            properties = {
                            surveyId = surveyId
                            questionId = questionId
                            answer = answer
                         }
            links = [||]
        }

    let getPostAnswerResponseA (e: AddNewAnswerError) =
        match e with
            | AnswerNotForSurvey -> 
                (getError "AnswerNotForSurvey" :> obj, HttpStatusCode.BadRequest)
            | AnswerNotForNextQuestion -> 
                (getError "AnswerNotForNextQuestion" :> obj, HttpStatusCode.BadRequest)
            | ErrorGettingNext x ->
                (getError "ErrorGettingNext" :> obj, HttpStatusCode.BadRequest)
     
        
    let getPostAnswerResponse (x: RopResult<Guid*Guid*bool, PostAnswerError>) =
       match x with
        | Success (s,_) -> ((toAnswerResponse s) :> obj, HttpStatusCode.Created) 
        | Failure (InvalidSurveyIdP::_) -> (getError "InvalidSurveyId" :> obj, HttpStatusCode.BadRequest)
        | Failure (InvalidQuestionIdP::_) -> (getError "InvalidQuestionId" :> obj, HttpStatusCode.BadRequest)
        | Failure (InvalidBodyP::_) -> (getError "InvalidBody" :> obj, HttpStatusCode.BadRequest)
        | Failure (SurveyDoesNotExistP id::_) -> (getError "SurveyDoesNotExist" :> obj, HttpStatusCode.BadRequest)
        | Failure (AddNewAnswerError e::_) -> getPostAnswerResponseA e
        | Failure [] ->
            ("Unknown Failure" :> obj, 
             HttpStatusCode.InternalServerError)

    let getNextQuestionResponse (x: RopResult<SurveyQuestion, GetNextQuestionError>) =
       match x with
        | Success (s,_) -> (toSurveyResponse s:> obj, HttpStatusCode.OK)
        | Failure (InvalidSurveyId::_) -> (getError "InvalidSurveyId" :> obj, HttpStatusCode.BadRequest)
        | Failure (SurveyDoesNotExist id::_) -> (getError "SurveyDoesNotExist" :> obj, HttpStatusCode.NotFound)
        | Failure (NoNextResult (n,id)::_) -> 
            match n with
                | SurveyComplete -> 
                    (getSurveyStatus id "surveyComplete" |> getSurveyStatusResponse :> obj, 
                     HttpStatusCode.OK)
                | SurveyIncompleteRequiringChangesToProgress-> 
                    (getSurveyStatus id "furtherWorkRequired" |> getSurveyStatusResponse :> obj, 
                     HttpStatusCode.OK)
                | ErrorNoSurveyQuestions -> 
                    (getError "NoSurveyQuestions" :> obj, 
                     HttpStatusCode.InternalServerError)
        | Failure [] ->
            ("Unknown Failure" :> obj, 
             HttpStatusCode.InternalServerError)

    let exceptionToNone f =
        let result = 
            try
                Some (f())
            with
                | _ -> None
        result

    do x.Get.["/api/survey/helloworld"] <- fun dd ->
        "hello back at ya" :> obj

    do x.Post.["/api/survey/helloworld"] <- fun dd ->
        let posted = exceptionToNone (fun () -> x.Bind<PostAnswerBody>())
        match posted with
            | Some p -> (p.answer.ToString())  :> obj
            | None -> 
                x.Response.AsJson("Unable to deserialize", HttpStatusCode.BadRequest) :> obj

    do x.Get.["/api/survey/{surveyId}/nextquestion"] <- fun dd ->
        let lGetSurvey = fun x -> SurveyDal.getSurvey x |> failIfNone (SurveyDoesNotExist x)
        let mGetNext id a b = SurveyDomain.getNext a b |> mapMessagesR (fun e -> NoNextResult (e,id))
        let result = rop {
            let! surveyIdStr = (getParamValue dd "surveyId")
                                |> failIfNone InvalidSurveyId
            let! surveyId = parseGuid surveyIdStr
                                |> failIfNone InvalidSurveyId
            let! survey = lGetSurvey surveyId
            let answers = SurveyDal.getAnswers surveyId
            let nextQuestion = mGetNext surveyId survey answers
            return! nextQuestion
        }
        let (body, statusCode) = getNextQuestionResponse result
        x.Response.AsJson(body, statusCode) :> obj

        // this one is next.
        //POST /api/survey/123/question/x/answer
        //	posts a y/n answer for 123.x, if it is allowed.
    do x.Post.["/api/survey/{surveyId}/question/{questionId}/answer"] <- fun dd ->
        let result = rop {
            let! surveyIdStr = (getParamValue dd "surveyId")
                                |> failIfNone InvalidSurveyIdP
            let! surveyId = parseGuid surveyIdStr
                                |> failIfNone InvalidSurveyIdP
            let! questionIdStr = (getParamValue dd "questionId")
                                |> failIfNone InvalidQuestionIdP
            let! questionId = parseGuid questionIdStr
                                |> failIfNone InvalidQuestionIdP
            let! body = exceptionToNone (fun () -> x.Bind<PostAnswerBody>())
                                |> failIfNone InvalidBodyP

            let answer = {
                questionId = questionId
                result = body.answer
            }

            let! survey = SurveyDal.getSurvey surveyId |> failIfNone (SurveyDoesNotExistP surveyId)
            let answers = SurveyDal.getAnswers surveyId

            let! addResult = addNewAnswer survey answers answer
                                |> mapMessagesR (fun e -> AddNewAnswerError e)

            SurveyDal.addAnswer surveyId answer

            return (surveyId, questionId, body.answer)
        }

        let (body, statusCode) = getPostAnswerResponse result
        x.Response.AsJson(body, statusCode) :> obj
