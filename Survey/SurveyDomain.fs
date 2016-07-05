namespace Survey

module SurveyDomain = 
    open System
    open Hombredequeso.Rop

    module Question =
        type Error =
            | TooShort
            | TooLong

        type T = Question of string
        let create (s: string) =
            match s with
            | _ when s.Length < 4 -> TooShort |> fail
            | _ when s.Length > 512 -> TooLong |> fail
            | _ -> Question s |> succeed
        let value (Question q) = q

    type SurveyQuestion = {
        id: Guid
        question: Question.T
    }

    type Section = {
        Questions: SurveyQuestion list
    }

    type Survey = {
        id: Guid
        Sections: Section list
    }

    type Answer = {
        id: Guid
        result: bool
    }

    type NoNextResult =
        | SurveyComplete
        | SurveyIncompleteRequiringChangesToProgress
        | ErrorNoSurveyQuestions

    // surveyIndex*questionIndex*SurveyQuestion
    let flatten (s:Survey): (int*int*SurveyQuestion) list =
        let indexItems i s = i,s
        let iSections = s.Sections |> List.mapi indexItems
        let getNewAcc acc ((sectioni, s):int*Section) = 
            let iQuestions = s.Questions |> List.mapi (fun i s -> sectioni, i, s)
            acc @ iQuestions 
        let iiAnswer = List.fold (fun acc e -> getNewAcc acc e) [] iSections
        iiAnswer

    // surveyIndex*questionIndex*SurveyQuestion*Answer option
    let correlate (questions: (int*int*SurveyQuestion)list) (answers: Answer list) =
       let getAnswer id = answers |> List.tryFind (fun a -> a.id = id) |> Option.map (fun a -> a.result)
       questions |> List.map (fun (sI, qI, q) -> (sI, qI, q, (getAnswer q.id)))

    let rec getRemainingFromNextSection 
            (x: (int*int*SurveyQuestion*bool option)list) (currentSection: int) =
        match x with
            | [] -> []
            | (iSection,_,_,_)::remaining when iSection = currentSection -> getRemainingFromNextSection remaining currentSection
            | (iSection,_,_,_)::remaining (* when iSection <> currentSection *)-> x

    let rec getNextInner (x: (int*int*SurveyQuestion*bool option)list) : RopResult<SurveyQuestion, NoNextResult> =
        match x with
            | [] -> SurveyComplete |> fail
            | (_,_,q,None)::_ -> q |> succeed
            | (iSection,_,q,Some false)::remainingQuestions -> getNextInner (getRemainingFromNextSection remainingQuestions iSection)
            | (_,_,q,Some true)::remainingQuestions-> getNextInner remainingQuestions

    let getNext (s:Survey) (answers: Answer list) : RopResult<SurveyQuestion, NoNextResult> =
        let qns = flatten s
        let qAndA = correlate qns answers

        match qAndA with
            | [] -> ErrorNoSurveyQuestions |> fail
            | x -> getNextInner x


