namespace Survey

module SurveyDomain = 
    open System

    module Question =
        type T = Question of string
        let create (s: string) =
            Question s
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

    //let getSection (survey: Survey) (question: SurveyQuestion) 
    //        : Section option =
    //    None 

    //type GetNextQuestionResult =
    //    | Question of SurveyQuestion
    //    | EndOfSurvey
    //    | QuestionNotInSurvey

    //let getNextQuestion (survey: Survey) (lastQuestionAnswered: Guid) (answer: bool) : GetNextQuestionResult =
    //    EndOfSurvey

