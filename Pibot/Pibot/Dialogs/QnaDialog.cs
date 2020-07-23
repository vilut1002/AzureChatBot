// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;

namespace Pibot.Dialogs
{
    public class QnaDialog : ComponentDialog
    {
        private readonly ILogger<QnaDialog> _logger;
        private readonly IBotServices _botServices;

        public QnaDialog(IBotServices botServices, ILogger<QnaDialog> logger, UserState userState)
            : base(nameof(QnaDialog))
        {
            _logger = logger;
            _botServices = botServices;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("") }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var topIntent = recognizerResult.GetTopScoringIntent();

            if (topIntent.intent == "예약")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("헌혈 예약 메뉴로 이동합니다."), cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            }

            else if (topIntent.intent == "종료")
                return await stepContext.EndDialogAsync(null, cancellationToken);

            else
            {
                switch (topIntent.intent)
                {
                    case "장소":
                        if (recognizerResult.Entities.Count > 1)
                        {
                            var entity = recognizerResult.Entities;
                            string json = @$"{recognizerResult.Entities}";
                            JObject jobj = JObject.Parse(json);
                            stepContext.Values["place"] = jobj["$instance"]["Place"][0]["text"].ToString();
                            List<Attachment> cards = new List<Attachment>();

                            switch (stepContext.Values["place"])
                            {
                                case "강북구":
                                case "노원구":
                                case "성북구":
                                    await stepContext.Context.SendActivityAsync($"강북구, 노원구, 성북구 헌혈의집 리스트입니다.");
                                    cards.Add(MakeMapCards("서울동부혈액원", 37.64695, 127.062077, "서울 노원구 동일로 1329\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("노해로센터", 37.654154, 127.061429, "서울 노원구 노해로 480 조광빌딩 2층").ToAttachment());
                                    cards.Add(MakeMapCards("노원센터", 37.655998, 127.062798, "서울 노원구 상계로 70, 화랑빌딩 7층").ToAttachment());
                                    cards.Add(MakeMapCards("돈암센터", 37.591916, 127.017705, "서울 성북구 동소문로 20다길 17, 광희빌딩 4층").ToAttachment());
                                    cards.Add(MakeMapCards("고려대앞센터", 37.585649, 127.029497, "서울 성북구 인촌로24길 11, 2층\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("수유센터", 37.585649, 127.029497, "서울 강북구 도봉로 325, 수유리교회 4층").ToAttachment());
                                    break;
                                case "광진구":
                                case "동대문구":
                                case "성동구":
                                case "중랑구":
                                    await stepContext.Context.SendActivityAsync($"광진구, 동대문구, 성동구, 중랑구 헌혈의집 리스트입니다.");
                                    cards.Add(MakeMapCards("동서울2센터", 37.534321, 127.094419, "서울 광진구 강변역로 50 동서울터미널내 1층 114호").ToAttachment());
                                    cards.Add(MakeMapCards("건대역센터", 37.540734, 127.070432, "서울 광진구 동일로22길 115, 4층\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("회기센터", 37.589647, 127.056803, "서울 동대문구 회기로 188, 두리빌딩 5층").ToAttachment());
                                    cards.Add(MakeMapCards("한양대역센터", 37.555806, 127.043850, "서울 성동구 왕십리로 206, 지하철2호선 한양대역점포 209-4호").ToAttachment());
                                    cards.Add(MakeMapCards("망우역센터", 37.598433, 127.091648, "서울 중랑구 망우로 353 상봉 프레미어스엠코 C동 302호").ToAttachment());
                                    break;
                                case "은평구":
                                case "서대문구":
                                case "마포구":
                                    await stepContext.Context.SendActivityAsync($"은평구, 서대문구, 마포구 헌혈의집 리스트입니다.");
                                    cards.Add(MakeMapCards("연신내센터", 37.619058, 126.920193, "서울 은평구 통일로 855-8, 연신내진원와이타운 4층 401호").ToAttachment());
                                    cards.Add(MakeMapCards("신촌연대앞센터", 37.557314, 126.937379, "서울 서대문구 명물길 6, 신촌빌딩 8층").ToAttachment());
                                    cards.Add(MakeMapCards("신촌센터", 37.555600, 126.937854, "서울 서대문구 신촌로 107, 세인빌딩 2층").ToAttachment());
                                    cards.Add(MakeMapCards("홍대센터", 37.555794, 126.922844, "서울 마포구 양화로 152, 대화빌딩 6층").ToAttachment());
                                    break;
                                case "종로구":
                                case "중구":
                                    await stepContext.Context.SendActivityAsync($"종로구, 중구 헌혈의집 리스트입니다.");
                                    cards.Add(MakeMapCards("대학로센터", 37.583349, 127.000063, "서울 종로구 대명길 26 3층").ToAttachment());
                                    cards.Add(MakeMapCards("광화문센터", 37.570624, 126.979816, "서울 종로구 종로 33 그랑서울 2층").ToAttachment());
                                    cards.Add(MakeMapCards("서울역센터", 37.557422, 126.969516, "서울 중구 청파로 426").ToAttachment());
                                    break;
                                case "강동구":
                                case "송파구":
                                case "강남구":
                                case "서초구":
                                    await stepContext.Context.SendActivityAsync($"강동구, 송파구, 강남구, 서초구 헌혈의집 리스트입니다.");
                                    cards.Add(MakeMapCards("천호센터", 37.537906, 127.126966, "서울 강동구 천호대로 1033, 8층\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("강남센터", 37.501315, 127.025498, "서울 서초구 강남대로 437, 7층\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("서울남부센터", 37.481938, 127.048888, "서울특별시 강남구 개포로31길 48\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("강남2센터", 37.496620, 127.028661, "서울 강남구 역삼동 강남대로 378, 9층").ToAttachment());
                                    cards.Add(MakeMapCards("코엑스센터", 37.511176, 127.059911, "서울 강남구 봉은사로 524, 코엑스몰R7").ToAttachment());
                                    cards.Add(MakeMapCards("잠실역센터", 37.512501, 127.101131, "서울 송파구 올림픽로 240(지하광장7호)").ToAttachment());
                                    break;
                                case "동작구":
                                case "관악구":
                                case "영등포구":
                                    await stepContext.Context.SendActivityAsync($"동작구, 관악구, 영등포구 헌혈의집 리스트입니다.");
                                    cards.Add(MakeMapCards("이수센터", 37.486312, 126.981671, "서울 동작구 동작대로 109, 경문빌딩 3층").ToAttachment());
                                    cards.Add(MakeMapCards("노량진역센터", 37.513336, 126.942966, "서울 동작구 노량진로 154\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("서울대역센터", 37.478676, 126.952557, "서울 관악구 관악로 152, 2층\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("서울대학교센터", 37.463488, 126.949454, "서울 관악구 관악로 1, 서울대학교 67동 두레문예관 103-1호").ToAttachment());
                                    cards.Add(MakeMapCards("대방역센터", 37.513289, 126.926285, "서울 영등포구 여의대방로 300\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("영등포센터", 37.516723, 126.906099, "서울 영등포구 영중로 3\r\n　").ToAttachment());
                                    break;
                                case "강서구":
                                case "양천구":
                                case "구로구":
                                    await stepContext.Context.SendActivityAsync($"강서구, 양천구, 구로구 헌혈의집 리스트입니다.");
                                    cards.Add(MakeMapCards("서울중앙혈액원", 37.548028, 126.870858, "서울 강서구 공항대로 591 대한적십자사 서울중앙혈액원 3층").ToAttachment());
                                    cards.Add(MakeMapCards("우장산역센터", 37.547842, 126.835823, "서울 강서구 강서로45길 5, 2층\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("발산역센터", 37.559783, 126.838418, "서울 강서구 강서로 385, 우성SB타워 507호").ToAttachment());
                                    cards.Add(MakeMapCards("목동센터", 37.528160, 126.875776, "서울 양천구 목동동로 293, 현대41타워 지하1층 B-02호").ToAttachment());
                                    cards.Add(MakeMapCards("구로디지털단지역센터", 37.485144, 126.901529, "서울 구로구 도림천로 477\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("신도림테크노마트센터", 37.507018, 126.890208, "서울 구로구 새말로 97, 신도림테크노마트 지하광장").ToAttachment());
                                    break;
                                case "성남시":
                                case "수원시":
                                    await stepContext.Context.SendActivityAsync($"성남시, 수원시 헌혈의집 리스트입니다.");
                                    cards.Add(MakeMapCards("야탑센터", 37.411617, 127.127596, "경기도 성남시 분당구 야탑로 69번길 24-8, 3층").ToAttachment());
                                    cards.Add(MakeMapCards("서현센터", 37.384133, 127.121495, "경기도 성남시 분당구 분당로 53번길 11, 서원빌딩 4층").ToAttachment());
                                    cards.Add(MakeMapCards("경기혈액원", 37.259395, 127.030294, "경기도 수원시 권선구 권광로 129\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("수원시청역센터", 37.263872, 127.03227, "경기 수원시 팔달구 권광로 181 씨네파크 2층 204호").ToAttachment());
                                    cards.Add(MakeMapCards("수원역센터", 37.266101, 127.001606, "경기도 수원시 팔달구 덕영대로 923-1, 4층").ToAttachment());
                                    break;
                                case "군포시":
                                case "안산시":
                                case "안양시":
                                    await stepContext.Context.SendActivityAsync($"안양, 군포, 안산 헌혈의집 리스트입니다.");
                                    cards.Add(MakeMapCards("평촌센터", 37.389693, 126.95106, "경기도 안양시 동안구 동안로 130\r\n　").ToAttachment());
                                    cards.Add(MakeMapCards("안양센터", 37.400566, 126.921963, "경기도 안양시 만안구 만안로 223번길 13, 3층").ToAttachment());
                                    cards.Add(MakeMapCards("산본센터", 37.359666, 126.932064, "경기도 군포시 산본로 323번길 16-14, 3층").ToAttachment());
                                    cards.Add(MakeMapCards("한대앞역센터", 37.309073, 126.853535, "경기 안산시 상록구 광덕4로 391, 공영주차장 내").ToAttachment());
                                    cards.Add(MakeMapCards("동탄센터", 37.206879, 127.073016, "경기도 화성시 동탄반석로 204 동탄제일프라자 205호").ToAttachment());
                                    break;
                                case "화성시":
                                case "용인시":
                                case "평택시":
                                    await stepContext.Context.SendActivityAsync($"화성 용인 평택 헌혈의집 리스트입니다.");
                                    cards.Add(MakeMapCards("용인센터", 37.234822, 127.203591, "경기도 용인시 처인구 금령로 64, 201, 202호").ToAttachment());
                                    cards.Add(MakeMapCards("수지센터", 37.323712, 127.096447, "경기도 용인시 수지구 풍덕천로 133 금오플라자 4층").ToAttachment());
                                    cards.Add(MakeMapCards("평택역센터", 36.990951, 127.08673, "경기도 평택시 평택로 39번길 36, 2층").ToAttachment());
                                    break;
                                default:
                                    await stepContext.Context.SendActivityAsync($"서울은 '서대문구 헌혈의집' 등 구단위로, 경기는 '성남시 헌혈의집' 등 시단위로 적어주시면 제가 더 알기 쉬워요!");
                                    break;
                            }
                            var reply = MessageFactory.Attachment(cards);
                            reply.Attachments = cards;
                            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                        }
                        else
                            await stepContext.Context.SendActivityAsync($"서울은 '서대문구 헌혈의집' 등 구단위로, 경기는 '성남시 헌혈의집' 등 시단위로 적어주시면 제가 더 알기 쉬워요!");
                        break;
                    case "인사":
                        await stepContext.Context.SendActivityAsync($"안녕하세요ღ'ᴗ'ღ 헌혈의 모든 것을 알려드리는 챗봇, 피봇입니다!");
                        break;
                    case "조건":
                        await stepContext.Context.SendActivityAsync($"헌혈 가능한 나이는 만 16세부터 69세까지이며, 헌혈 가능한 체중은 남자 50kg 이상, 여자 45kg 이상입니다.");
                        break;
                    case "운영_시간":
                        await stepContext.Context.SendActivityAsync($"헌혈의 집 운영시간은 8:00 ~ 20:00 이며 지점마다 다를 수 있으니 확인 부탁드립니다.");
                        break;
                    case "준비물":
                        await stepContext.Context.SendActivityAsync($"신분증을 준비해주세요. (주민등록증, 여권 등 관공서 및 공공기관이 발행한 것으로 사진과 주민등록번호가 확인가능한 것)");
                        break;
                    case "약물":
                        await stepContext.Context.SendActivityAsync($"약물 복용과 관련한 헌혈 가능조건은 다음과 같습니다.\n\n" +
                            $" - 건선 치료제 복용 후 3년 경과(일부는 영구 헌혈금지)\r\n" +
                            $" - 전립선비대증 치료제 복용 후 1개월 또는 6개월 경과\r\n" +
                            $" - 탈모증 치료제 복용 후 1개월 경과\r\n" +
                            $" - 여드름 치료제 복용 후 1개월 경과\r\n" +
                            $" - 기타 헌혈금지약물 복용 후 일정기간 경과");
                        break;

                    default:
                        var didntUnderstandMessageText = $"죄송합니다. 이해하지 못했어요. ";
                        var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                        await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                        break;
                }

                return await stepContext.BeginDialogAsync(nameof(QnaDialog), new BookingDetails(), cancellationToken);
            }

        }
        private static HeroCard MakeMapCards(string name, double latitude, double longitude, string address)
        {
            HeroCard card = new HeroCard(
                            title: name,
                            images: new CardImage[] { new CardImage() { Url = MakeMap(latitude, longitude) } },
                            text: address,
		tap:  new CardAction() { Value = $"http://maps.google.com/maps?q={ latitude},{longitude}",Type = "openUrl", } 
                            );

            return card;
        }
        private static String MakeMap(double latitude, double longitude)
        {
            string latitudeStr = latitude.ToString();
            string longitudeStr = longitude.ToString();
            return $"http://maps.google.com/maps/api/staticmap?center={ latitudeStr },{ longitudeStr}&zoom=16&size=512x512&maptype=roadmap&markers=color:red%7C{ latitudeStr },{ longitudeStr }&sensor=false&key=AIzaSyCUGkyf6nzMobitlUprUzDNIqb2GTDj2lk";
        }
    }

}


