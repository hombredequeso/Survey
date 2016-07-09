namespace Survey

module SurveyDal = 
    open System
    open System.Collections.Generic
    open Hombredequeso.Rop
    open SurveyDomain

    let answers = new List<Answer>()

    let getAnswers (surveyid: Guid): Answer list =
        List.ofSeq answers

    let addAnswer surveyId a =
        answers.Add(a)

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

    let getSurvey (surveyId:Guid) = 
        Some sampleSurvey
