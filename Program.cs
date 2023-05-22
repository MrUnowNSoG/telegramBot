using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text;

namespace Anti_war_mini_bot {
    class Program {


        //Main setting
        private static string token { get; set; } = "xxxxxxxxxxKeyxxxxxxxxxxx";
        private static        TelegramBotClient     client;

        //
        public static Dictionary<long, Human> all_Human = new Dictionary<long, Human>();
        public static List<Force> all_Force = new List<Force>();
        public static List<Human> top_Human = new List<Human>();


        //
        private static bool WorkTimerB = false;
        private static bool workBot = true;
        private static int mainIndexForce = 0;

        //
        private static int countBan = 5;
        private static int countWin = 15;
        private static string[] comentarBots = { "Фейкова інформація!!!", "Розповідають не правдиві байки про Українців!", " За УКРАЇНУ!" };

        static void Main(string[] args) {
            
            #region Translate UA
            //NuGet pkg: System.Text.Encoding.CodePages
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var enc1251 = Encoding.GetEncoding(1251);

            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            System.Console.InputEncoding = enc1251;
            #endregion

            //Create Bot
            client = new TelegramBotClient(token);
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;


            //Admin Panel
            bool whileWork = true;
            while(whileWork) {
                Console.WriteLine("\n\n Work Console! \n Menu - 0");

                string indexWork = Console.ReadLine();

                switch(indexWork) {
                    case "0":
                        Console.WriteLine("Iнфа: 0 - Menu \n 1 - Create New Top \n 2 - Check Time  і обнова \n 3 - Повідомлення всім учасникам бота \n 4 - Офнути бота без повідомлення \n 5 - Включити бота без повідомлення \n 6 - Виключити бота з повідомлння \n 7 - Включити бота з повідомлення \n 8 - Загрузити всю інформацію\n 9 - Зберегти всю інформацію \n 10 - Exit");
                        break;

                    case "1":
                        CreateNewTop();
                        Console.WriteLine("Iнфа: Новий топ створено!");
                        break;

                    case "2":
                        DateTime DT = DateTime.Now;
                        Console.WriteLine(DT.Minute);
                        Console.Write("\n\n Обновлення данних відбувається!\n \n");
                        CreateNewTop();
                        CheckForce();
                        break;

                    case "3":
                        bool workText = true;
                        string text = "";
                        Console.WriteLine("Iнфа: Меню Текста \n 1 - Ввести рядок \n 2 - Відправити рядок \n 3 - Назад");

                        while (workText) {
                            Console.WriteLine(" \nIнфа: Дія №: ");
                            string temp = Console.ReadLine();
                            switch(temp) {
                                case "1":
                                    Console.WriteLine("Iнфа: Введіть 1 рядок: ");
                                    temp = Console.ReadLine();
                                    text += temp + "\n";
                                    break;

                                case "2":
                                    Console.WriteLine("\n \n Iнфа: Текст повідомлення: ");
                                    Console.WriteLine(text);
                                    SendTextAllPeople(text);
                                    Console.WriteLine("Iнфа: Повідомлення відправлено!");
                                    workText = false;
                                    break;

                                case "3":
                                    Console.WriteLine("Iнфа: Повернулися назад");
                                    workText = false;
                                    break;

                                default:
                                    workText = false;
                                    break;
                            }
                        }
                        break;


                    case "4":
                        workBot = false;
                        Console.WriteLine("Iнфа: Бот офнутий!");
                        break;

                    case "5":
                        workBot = true;
                        Console.WriteLine("Iнфа: Бот включений!");
                        break;

                    case "6":
                        workBot = false;
                        SendTextAllPeople("Бот припиняє свою роботу на деякий час =(");
                        Console.WriteLine("Iнфа: Бот офнутий з інфой!");
                        break;

                    case "7":
                        workBot = true;
                        SendTextAllPeople("Бот відновив свою роботу =) ");
                        Console.WriteLine("Iнфа: Бот включений з інфой!");
                        break;

                    case "8":
                        Download();
                        Console.WriteLine("Iнфа: Інформація загружена!");
                        break;

                    case "9":
                        Save();
                        Console.WriteLine("Iнфа: Інформація збережена!");
                        break;

                    case "10":
                        whileWork = false;
                        Console.WriteLine("Iнфа: Програма виключена!");
                        break;


                    default:
                        Console.WriteLine("Iнфа: Error Console Admin");
                        whileWork = false;
                        break;
                }
            }

            //Stop Bot
            client.StartReceiving();

        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e) {

            if (workBot) {

                //Temp variables
                var msg = e.Message;
                Human temp_HM;

                if (msg.Text != null) {

                    WorkSwitchAsynk(e);

                    //Перевірка на потрібні повідомлення
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        //Nick
                        if (msg.Text != "Так!" &&  msg.Text != "/start" && msg.Text != "Змінити!" && temp_HM.Status_HM == Human.States.NewNick) {
                            temp_HM.Fake_Name = msg.Text;
                            await client.SendTextMessageAsync(msg.Chat.Id,
                               $"Ваше нове і'мя {temp_HM.Fake_Name}?",
                               replyMarkup: GetButton(3));
                        }

                        //Me
                        if (msg.Text != "Написати розробнику!" && msg.Text != "/start"  && temp_HM.Status_HM == Human.States.SendMessage) {
                            await client.SendTextMessageAsync(msg.Chat.Id, $"Повідомлення відправлено!", replyMarkup: GetButton(5));
                            await client.SendTextMessageAsync(all_Human.ElementAt(0).Key, $"Прийшло повідомлення від: {msg.Chat.FirstName} {msg.Chat.LastName} \n {msg.Chat.Id} \n {msg.Chat.Username} \n Повідомлення: \n {msg.Text}");
                            temp_HM.Status_HM = Human.States.Work;
                        }

                        //New Linc
                        if (msg.Text != "Відправте силку(або введіть: /start щоб повернутися назад):" && msg.Text != "/start" && temp_HM.BanCount < 4 && temp_HM.Status_HM == Human.States.SendLinc) {
                            //
                            foreach (var obj in all_Force) {
                                if (obj.ForceMain == msg.Text) {
                                    await client.SendTextMessageAsync(msg.Chat.Id, $"Таку силку вже добавили😔", replyMarkup: GetButton(5));
                                    temp_HM.Status_HM = Human.States.Work;
                                    return;
                                }
                            }

                            Console.WriteLine("Error! " + msg.Text);
                            all_Force.Add(new Force(msg.Text, mainIndexForce, temp_HM.Chat_Id));
                            temp_HM.indexForceAdd = mainIndexForce;
                            temp_HM.Status_HM = Human.States.SentType;
                            await client.SendTextMessageAsync(msg.Chat.Id, $"Вибиріть тип ресурсу: ", replyMarkup: GetButton(7));
                            mainIndexForce++;

                        }

                        //Add info comment
                        if (msg.Text != "Напишіть свій коментар😃 або бот напише свій: " && msg.Text != "/start"  && msg.Text != "Напишіть свій коментар😃: " && temp_HM.Status_HM == Human.States.SendComment) {
                            all_Force[temp_HM.indexForceAdd].Commentar = msg.Text;
                            temp_HM.score += 30;
                            temp_HM.Status_HM = Human.States.Work;
                            await client.SendTextMessageAsync(msg.Chat.Id, $"Дякую за допомогу!!!😃 \n Вам нараховано 30 поінтів!", replyMarkup: GetButton(5));
                        }
                    }

                    Console.WriteLine($" \n Прийшло повідомлення: {msg.Text}. \n Від: {msg.Chat.FirstName} {msg.Chat.LastName} \n Телега: {msg.Chat.Username} \n {msg.Chat.Id}");

                    #region Example
                    /* 
                    await client.SendTextMessageAsync(msg.Chat.Id, msg.Text, replyMarkup: GetButton(1));

                    /* відповідь просто
                    await client.SendTextMessageAsync(msg.Chat.Id, msg.Text, replyToMessageId: msg.MessageId);
                    */

                    /* cтикер силка з гугла
                    var stic = await client.SendStickerAsync(
                        chatId: msg.Chat.Id,
                        sticker: "https://cdn.tlgrm.app/stickers/c62/4a8/c624a88d-1fe3-403a-b41a-3cdb9bf05b8a/192/10.webp");
                    */

                    #endregion
                }

                WorkTimerAsync();
            }
        }

        //Асинхронна обертка
        private static async Task WorkSwitchAsynk(MessageEventArgs e) {
            await Task.Run(() => WorkSwitch(e));
        }


        private static async void WorkSwitch(MessageEventArgs e) {
            var msg = e.Message;
            Human temp_HM;

            switch (msg.Text) {

                #region Start
                case "/start":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        switch (temp_HM.Status_HM) {
                            case Human.States.NewUser:
                                await client.SendTextMessageAsync(msg.Chat.Id,
                                    $"Давайте пройдемо швидке налаштування.Чи бажаєте ви змінити своє і'мя {msg.Chat.Username}?(Може відображатися у всіх учасників в топах)",
                                    replyMarkup: GetButton(2));
                                break;

                            case Human.States.NewNick:
                                await client.SendTextMessageAsync(msg.Chat.Id,
                                      "Ок, введіть своє нове позивне:", replyMarkup: new ReplyKeyboardRemove());
                                break;

                            case Human.States.SettingNet:
                                await client.SendTextMessageAsync(msg.Chat.Id, "Давайте виберемо соц-мережі в яких вас немає!(❔❔❔) \n Щоб я не давав вам не робочі посилання!", replyMarkup: GetButton(4));
                                break;

                            default:
                                temp_HM.Status_HM = Human.States.Work;
                                await client.SendTextMessageAsync(msg.Chat.Id, "Давайте почнем нашу війну!", replyMarkup: GetButton(5));
                                break;
                        }

                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id,
                        "Вас вітає Anti-war mini bot👋👋👋 \n Який призначений для блокування російських фейків в інтернеті!(Закидуванн репортами фейкові ресурси)Проте ці цілі я не можу досягти сам😔.Тому я був би дуже радий, якщо б ти допоміг мені.Чи готовий ти разом з іншими 🇺🇦 Українцями 🇺🇦 стати захисником кіберпростору?",
                        replyMarkup: GetButton(1));
                    }
                    break;


                case "Ні!😔":
                    await client.SendTextMessageAsync(msg.Chat.Id,
                        "Це сумно проте... \n  Вас вітає Anti-war mini bot👋👋👋 \n Який призначений для блокування російських фейків в інтернеті!Проте ці цілі я не можу досягти сам😔.Тому я був би дуже радий, якщо б ти допоміг мені.Чи готовий ти разом з іншими 🇺🇦 Українцями 🇺🇦 стати захисником кіберпростору?",
                        replyMarkup: GetButton(1));
                    break;
                #endregion

                #region SettingNick
                case "Так⚔️":
                    if (!all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        all_Human.Add(msg.Chat.Id, new Human(msg.Chat.Id, msg.Chat.Username));
                        await client.SendTextMessageAsync(msg.Chat.Id,
                            $"Давайте пройдемо швидке налаштування.Чи бажаєте ви змінити своє позивне {msg.Chat.Username}?(Може відображатися у всіх учасників в топах)",
                            replyMarkup: GetButton(2));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Змінити свої налаштування!":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        await client.SendTextMessageAsync(msg.Chat.Id,
                            $"Давайте пройдемо швидке налаштування.Чи бажаєте ви змінити своє позивне {msg.Chat.Username}?(Може відображатися у всіх учасників в топах)",
                            replyMarkup: GetButton(2));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Змінити!":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        await client.SendTextMessageAsync(msg.Chat.Id,
                        "Добре, введіть своє нове позивне:", replyMarkup: new ReplyKeyboardRemove());
                        temp_HM.Status_HM = Human.States.NewNick;
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Залишити!😃":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        temp_HM.Status_HM = Human.States.SettingNet;
                        await client.SendTextMessageAsync(msg.Chat.Id, "Давайте виберемо соц-мережі в яких вас немає!(❔❔❔) \n Щоб я не давав вам не робочі посилання!", replyMarkup: GetButton(4));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Так!":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        temp_HM.Status_HM = Human.States.SettingNet;
                        await client.SendTextMessageAsync(msg.Chat.Id, "Давайте виберемо соц-мережі в яких вас немає!(❔❔❔) \n Щоб я не давав вам не робочі посилання!", replyMarkup: GetButton(4));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Ні, змінити":
                    await client.SendTextMessageAsync(msg.Chat.Id,
                            "Добре, введіть своє нове позивне:", replyMarkup: new ReplyKeyboardRemove());
                    break;
                #endregion

                #region SettingNet
                case "TikTok😔":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        temp_HM.TikTok = false;
                        await client.SendTextMessageAsync(msg.Chat.Id, "Давайте виберемо соц-мережі в яких вас немає!", replyMarkup: GetButton(4));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "YouTube😔":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        temp_HM.YouTube = false;
                        await client.SendTextMessageAsync(msg.Chat.Id, "Давайте виберемо соц-мережі в яких вас немає!", replyMarkup: GetButton(4));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Instagram😔":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        temp_HM.Insta = false;
                        await client.SendTextMessageAsync(msg.Chat.Id, "Давайте виберемо соц-мережі в яких вас немає!", replyMarkup: GetButton(4));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;


                case "Далі!":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        temp_HM.Status_HM = Human.States.Work;
                        await client.SendTextMessageAsync(msg.Chat.Id, "Давайте почнем нашу війну!", replyMarkup: GetButton(5));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;
                #endregion

                //Main
                case "Назад":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        await client.SendTextMessageAsync(msg.Chat.Id, "Давайте почнем нашу війну!", replyMarkup: GetButton(5));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Інформація що до кнопок🙌":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        await client.SendTextMessageAsync(msg.Chat.Id, "Пояснення, як працює бот!😅 \n Головна ціль це блокувати ворожі інтернет ресурси, зявдяки 🚫скаргам🚫, які ми можемо відправити! \n \n ⏹-----Кнопки-----⏹ \n 1) Блокувати - дає вам посилання, за яким ви можете перейти і кинути репорт на ворожі ресурси. \n 2) Топ - топ 10 людей за поінтами☝️ \n 3) Додати нове посилання - дає вам можливість додати свої посилання, які потрібно заблокувати(будуть відображатися у всіх учасників). \n 4) Інше - Написати розробнику і Змінити свої налаштування!", replyMarkup: GetButton(5));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;


                #region Інше
                case "Інше...":
                        await client.SendTextMessageAsync(msg.Chat.Id, "Деякі спец-можливості🤣", replyMarkup: GetButton(6));
                    break;

                case "Написати розробнику😳":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        temp_HM.Status_HM = Human.States.SendMessage;
                        await client.SendTextMessageAsync(msg.Chat.Id, "Відправте своє повідомлення(Вау!!!): ", replyMarkup: new ReplyKeyboardRemove());
                    }else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;
                #endregion

                #region NewLinc
                //Додати
                case "Додати нове посилання👍":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        if (temp_HM.BanCount < 4) {
                            await client.SendTextMessageAsync(msg.Chat.Id, "Відправте силку(або введіть: /start щоб повернутися назад):", replyMarkup: new ReplyKeyboardRemove());
                            temp_HM.Status_HM = Human.States.SendLinc;
                        } else {
                            await client.SendTextMessageAsync(msg.Chat.Id, "Ви занадто часто відправляэте посилання, які не працюють. Тому для вас ця функція заблокована!😡", replyMarkup: GetButton(5));
                        }
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "TikTok":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        if(temp_HM.indexForceAdd >= 0) {
                            if (temp_HM.Status_HM == Human.States.SentType) {
                                all_Force[temp_HM.indexForceAdd].typeForce = Force.TypeForce.TikTok;
                                await client.SendTextMessageAsync(msg.Chat.Id, "Напишіть свій коментар😃 або бот напише свій: ", replyMarkup: GetButton(10));
                                temp_HM.Status_HM = Human.States.SendTime;
                            }
                        } else await client.SendTextMessageAsync(msg.Chat.Id, "/start");

                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "YouTube":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        if (temp_HM.indexForceAdd >= 0) {
                            if (temp_HM.Status_HM == Human.States.SentType) {
                                all_Force[temp_HM.indexForceAdd].typeForce = Force.TypeForce.YouTube;
                                await client.SendTextMessageAsync(msg.Chat.Id, "Напишіть свій коментар😃 або бот напише свій: ", replyMarkup: GetButton(10));
                                temp_HM.Status_HM = Human.States.SendTime;
                            }
                        } else await client.SendTextMessageAsync(msg.Chat.Id, "/start");

                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Instagram":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        if (temp_HM.indexForceAdd >= 0) {
                            if (temp_HM.Status_HM == Human.States.SentType) {
                                all_Force[temp_HM.indexForceAdd].typeForce = Force.TypeForce.Insta;
                                await client.SendTextMessageAsync(msg.Chat.Id, "Напишіть свій коментар😃 або бот напише свій: ", replyMarkup: GetButton(10));
                                temp_HM.Status_HM = Human.States.SendTime;
                            }
                        } else await client.SendTextMessageAsync(msg.Chat.Id, "/start");

                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Telegram":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        if (temp_HM.indexForceAdd >= 0) {
                            if (temp_HM.Status_HM == Human.States.SentType) {
                                all_Force[temp_HM.indexForceAdd].typeForce = Force.TypeForce.Telegram;
                                await client.SendTextMessageAsync(msg.Chat.Id, "Напишіть свій коментар😃 або бот напише свій: ", replyMarkup: GetButton(10));
                                temp_HM.Status_HM = Human.States.SendTime;
                            }
                        } else await client.SendTextMessageAsync(msg.Chat.Id, "/start");

                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Інший":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        if (temp_HM.indexForceAdd >= 0) {
                            if (temp_HM.Status_HM == Human.States.SentType) {
                                all_Force[temp_HM.indexForceAdd].typeForce = Force.TypeForce.Other;
                                await client.SendTextMessageAsync(msg.Chat.Id, "Напишіть свій коментар😃 або бот напише свій: ", replyMarkup: GetButton(10));
                                temp_HM.Status_HM = Human.States.SendTime;
                            }
                        } else await client.SendTextMessageAsync(msg.Chat.Id, "/start");

                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Свій коментар":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        if (temp_HM.indexForceAdd >= 0) {
                            if (temp_HM.Status_HM == Human.States.SendTime) {
                                await client.SendTextMessageAsync(msg.Chat.Id, "Напишіть свій коментар😃: ", replyMarkup: new ReplyKeyboardRemove());
                                temp_HM.Status_HM = Human.States.SendComment;
                            }
                        } else await client.SendTextMessageAsync(msg.Chat.Id, "/start");

                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Бот напише сам🤖":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        if (temp_HM.indexForceAdd >= 0) {
                            if (temp_HM.Status_HM == Human.States.SendTime) {
                                all_Force[temp_HM.indexForceAdd].Commentar = comentarBots[new Random().Next(0, comentarBots.Length)];

                                temp_HM.score += 30;
                                temp_HM.Status_HM = Human.States.Work;
                                await client.SendTextMessageAsync(msg.Chat.Id, $"Дякую за допомогу!!!😃 \n Вам нараховано 30 поінтів!", replyMarkup: GetButton(5));
                            }
                        } else await client.SendTextMessageAsync(msg.Chat.Id, "/start");

                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;
                #endregion

                #region Top
                case "Топ⚔️":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        await client.SendTextMessageAsync(msg.Chat.Id, TopText(), replyMarkup: GetButton(5));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;
                #endregion

                #region Block
                case "Блокувати✊":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        await client.SendTextMessageAsync(msg.Chat.Id, "Ну що ж давай почнем!");
                        await SendLinc(temp_HM);
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Заблоковано!":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        temp_HM.indexForce++;
                        temp_HM.score += 5;
                        await SendLinc(temp_HM);
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Репорт🆘":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        await client.SendTextMessageAsync(msg.Chat.Id, "Вибиріть тип проблеми:", replyMarkup: GetButton(9));
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Акаунт Заблокований!":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                       
                        await client.SendTextMessageAsync(msg.Chat.Id, "Дякую за голос!", replyMarkup: GetButton(8));
                        temp_HM.score += 3;
                        all_Force[temp_HM.indexForce].countWin++;

                        temp_HM.indexForce++;
                        await SendLinc(temp_HM);
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;

                case "Має Тех.Проблеми":
                    if (all_Human.TryGetValue(msg.Chat.Id, out temp_HM)) {
                        await client.SendTextMessageAsync(msg.Chat.Id, "Дякую за голос!", replyMarkup: GetButton(8));
                        temp_HM.score += 3;
                        all_Force[temp_HM.indexForce].countProblem++;

                        temp_HM.indexForce++;
                        await SendLinc(temp_HM);
                    } else {
                        await client.SendTextMessageAsync(msg.Chat.Id, "/start");
                    }
                    break;
                    #endregion
            }
        }

        private static IReplyMarkup GetButton(int index) {

            switch(index) {
                case 1:
                    return new ReplyKeyboardMarkup( new List<KeyboardButton> { new KeyboardButton ("Так⚔️"), new KeyboardButton ("Ні!😔") });
                    break;

                case 2:
                    return new ReplyKeyboardMarkup(new List<KeyboardButton> { new KeyboardButton("Змінити!"), new KeyboardButton("Залишити!😃") });
                    break;

                case 3:
                    return new ReplyKeyboardMarkup(new List<KeyboardButton> { new KeyboardButton("Так!"), new KeyboardButton("Ні, змінити") });
                    break;

                case 4:
                    return new ReplyKeyboardMarkup {
                        Keyboard = new List<List<KeyboardButton>> {
                    new List<KeyboardButton> {new KeyboardButton { Text = "TikTok😔" }, new KeyboardButton { Text = "YouTube😔" } },
                    new List<KeyboardButton> {new KeyboardButton { Text = "Instagram😔" }, new KeyboardButton { Text = "Далі!"} }}
                    };


                case 5:
                    return new ReplyKeyboardMarkup {
                        Keyboard = new List<List<KeyboardButton>> {
                    new List<KeyboardButton> {new KeyboardButton { Text = "Блокувати✊" }, new KeyboardButton { Text = "Топ⚔️" } },
                    new List<KeyboardButton> {new KeyboardButton { Text = "Додати нове посилання👍" }, new KeyboardButton { Text = "Інше..."} },
                        new List<KeyboardButton> {new KeyboardButton { Text = "Інформація що до кнопок🙌" } }}
                    };

                case 6:
                    return new ReplyKeyboardMarkup {
                        Keyboard = new List<List<KeyboardButton>> {
                    new List<KeyboardButton> {new KeyboardButton { Text = "Написати розробнику😳" }, new KeyboardButton { Text = "Змінити свої налаштування!" } },
                        new List<KeyboardButton> {new KeyboardButton { Text = "Назад" } }}
                    };

                case 7:
                    return new ReplyKeyboardMarkup {
                        Keyboard = new List<List<KeyboardButton>> {
                    new List<KeyboardButton> {new KeyboardButton { Text = "TikTok" }, new KeyboardButton { Text = "YouTube" } },
                    new List<KeyboardButton> {new KeyboardButton { Text = "Instagram" }, new KeyboardButton { Text = "Telegram" } },
                        new List<KeyboardButton> {new KeyboardButton { Text = "Інший" }}}
                    };

                case 8:
                    return new ReplyKeyboardMarkup {
                        Keyboard = new List<List<KeyboardButton>> {
                    new List<KeyboardButton> {new KeyboardButton { Text = "Заблоковано!" }, new KeyboardButton { Text = "Назад" } },
                        new List<KeyboardButton> {new KeyboardButton { Text = "Репорт🆘" } }}
                    };

                case 9:
                    return new ReplyKeyboardMarkup {
                        Keyboard = new List<List<KeyboardButton>> {
                    new List<KeyboardButton> {new KeyboardButton { Text = "Акаунт Заблокований!" }, new KeyboardButton { Text = "Має Тех.Проблеми" } },
                        new List<KeyboardButton> {new KeyboardButton { Text = "Назад" }}}
                    };

                case 10:
                    return new ReplyKeyboardMarkup(new List<KeyboardButton> { new KeyboardButton("Свій коментар"), new KeyboardButton("Бот напише сам🤖") });
                    break;
                default:
                    break;
            }



            //Example
            return new ReplyKeyboardMarkup {
                Keyboard = new List<List<KeyboardButton>> {
                    new List<KeyboardButton> {new KeyboardButton { Text = "Button-1"}, new KeyboardButton { Text = "Button-2"} },
                    new List<KeyboardButton> {new KeyboardButton { Text = "Button-3"}, new KeyboardButton { Text = "Button-4"} }
                }
            };
        }

        private static void CreateNewTop() {
            top_Human.Clear();
            List<Human> top_Human_f = new List<Human>();
            
            for(int i = 0; i < all_Human.Count; i++) {
                top_Human_f.Add(all_Human.ElementAt(i).Value);
            }

            var TimeTop = top_Human_f.OrderByDescending(Human => Human.score);

            int count = 0;
            foreach(Human i in TimeTop) {
                if (count < 10) {
                    Console.WriteLine(55);
                    top_Human.Add(i);
                    count++;
                } else break;
            }
        }

        private static string TopText() {

            string ret = "🇺🇦Топ найсильныших воїнів!🇺🇦 \n";

            int count = 1;
            foreach(var i in top_Human) {
                if(count < 4) {
                    ret += $"⚔️ { i.Fake_Name} - {i.score} очків ⚔️ \n";
                } else {
                    ret += $"{count.ToString()}) { i.Fake_Name} - {i.score} очків \n";
                }
                count++;
            }
            ret += "Топ обновлюється кожних декілька годин!";
            return ret;
        }


        //Асинхронна обертка
        private static async Task WorkTimerAsync() {
            await Task.Run(() => WorkTimer());
        }

        private static void  WorkTimer() {
            DateTime DT = DateTime.Now;
            if (DT.Minute > 25 && DT.Minute < 35) {
                if (WorkTimerB == false) {
                    Console.Write("\n\n Обновлення данних відбувається!\n \n");
                    WorkTimerB = true;
                    CreateNewTop();
                    CheckForce();
                    Save();
                }

            } else WorkTimerB = false;
        }

        //Перевірка на бани

        private static void CheckForce() {
            foreach(var i in all_Force) {
                if (i.statsProblem == false && i.statsForseWin == false) {
                    if (i.BanTest(countBan)) all_Human[i.idHuman].BanCount++;
                }
                i.TestWin(countWin);
            }
        }

        private static void SendTextAllPeople(string text) {
            for(int i = 0; i < all_Human.Count; i++) {
                if (i % 25 == 0) Thread.Sleep(1500);
                client.SendTextMessageAsync(all_Human.ElementAt(i).Key, text);
            }
        }

        private static async Task SendLinc(Human temp_HM) {
            bool workFor = true;
            while (workFor) {
                if (temp_HM.indexForce < all_Force.Count) {
                    Force f_Tempe = all_Force[temp_HM.indexForce];

                    if (!f_Tempe.statsProblem && !f_Tempe.statsForseWin) {

                            switch (f_Tempe.typeForce) {
                                case Force.TypeForce.TikTok:
                                    if (temp_HM.TikTok) {
                                        await client.SendTextMessageAsync(temp_HM.Chat_Id, $"{f_Tempe.ForceMain} \n { f_Tempe.Commentar}", replyMarkup: GetButton(8));
                                        workFor = false;
                                    } else {
                                        temp_HM.indexForce++;
                                    }
                                    break;

                                case Force.TypeForce.YouTube:
                                    if (temp_HM.YouTube) {
                                        await client.SendTextMessageAsync(temp_HM.Chat_Id, $"{f_Tempe.ForceMain} \n { f_Tempe.Commentar}", replyMarkup: GetButton(8));
                                        workFor = false;
                                    } else {
                                        temp_HM.indexForce++;
                                    }
                                    break;

                                case Force.TypeForce.Insta:
                                    if (temp_HM.Insta) {
                                        await client.SendTextMessageAsync(temp_HM.Chat_Id, $"{f_Tempe.ForceMain} \n { f_Tempe.Commentar}", replyMarkup: GetButton(8));
                                        workFor = false;
                                    } else {
                                        temp_HM.indexForce++;
                                    }
                                    break;

                                default:
                                    await client.SendTextMessageAsync(temp_HM.Chat_Id, $"{f_Tempe.ForceMain} \n { f_Tempe.Commentar}", replyMarkup: GetButton(8));
                                    workFor = false;
                                    break;
                            }

                    } else {
                        temp_HM.indexForce++;
                    }

                } else {
                    await client.SendTextMessageAsync(temp_HM.Chat_Id, $"Посилання нажаль закінчилися, чекаємо коли добавлять... А може ви зможете додати?", replyMarkup: GetButton(5));
                    workFor = false;
                }
            }
        }

        private static void Save() {
            using (var swMain = new StreamWriter("BaseData/longUser.txt", false, encoding: System.Text.Encoding.UTF8)) {
                swMain.WriteLine(all_Human.Count);
                foreach (var i in all_Human) {
                    swMain.WriteLine(i.Key);

                    using (var sw = new StreamWriter("BaseData/users/" + i.Key.ToString() + ".txt", false, encoding: System.Text.Encoding.UTF8)) {
                        sw.WriteLine(i.Value.Chat_Id);
                        sw.WriteLine(i.Value.Real_Name);
                        sw.WriteLine(i.Value.Fake_Name);
                        sw.WriteLine(i.Value.BanCount);
                        sw.WriteLine(i.Value.TikTok);
                        sw.WriteLine(i.Value.YouTube);
                        sw.WriteLine(i.Value.Insta);
                        sw.WriteLine(i.Value.indexForce);
                        sw.WriteLine(((int)i.Value.Status_HM).ToString());
                        sw.WriteLine(i.Value.score);
                    }
                }
            }

            using (var swMain = new StreamWriter("BaseData/numForce.txt", false, encoding: System.Text.Encoding.UTF8)) {
                swMain.WriteLine(all_Force.Count.ToString());
            }

            for (int i = 0; i < all_Force.Count; i++) {
                using (var sw = new StreamWriter("BaseData/force/" + i.ToString() + ".txt", false, encoding: System.Text.Encoding.UTF8)) {
                    sw.WriteLine(all_Force[i].ForceMain);
                    sw.WriteLine(all_Force[i].Commentar);
                    sw.WriteLine(all_Force[i].index);
                    sw.WriteLine(all_Force[i].countWin);
                    sw.WriteLine(all_Force[i].countProblem);
                    sw.WriteLine(all_Force[i].idHuman);
                }
            }
        }

        private static void Download() {
            using (var srMain = new StreamReader("BaseData/longUser.txt", encoding: System.Text.Encoding.UTF8)) {
                int count = int.Parse(srMain.ReadLine());
                for(int i = 0; i < count; i++) { 
                    long index =  long.Parse(srMain.ReadLine());

                    Human temp_HM = new Human(0,"");
                    using (var sr = new StreamReader("BaseData/users/" + index.ToString() + ".txt", encoding: System.Text.Encoding.UTF8)) {
                        temp_HM.Chat_Id = long.Parse(sr.ReadLine());
                        temp_HM.Real_Name = sr.ReadLine();
                        temp_HM.Fake_Name = sr.ReadLine();
                        temp_HM.BanCount = int.Parse(sr.ReadLine());
                        temp_HM.TikTok = bool.Parse(sr.ReadLine());
                        temp_HM.YouTube = bool.Parse(sr.ReadLine());
                        temp_HM.Insta = bool.Parse(sr.ReadLine());
                        temp_HM.indexForce = int.Parse(sr.ReadLine());
                        temp_HM.Status_HM =(Human.States)int.Parse(sr.ReadLine());
                        temp_HM.score= int.Parse(sr.ReadLine());
                    }
                    temp_HM.indexForceAdd = -1;
                    all_Human.Add(index, temp_HM);
                }
            }

            int countForce = 0;
            using (var srMain = new StreamReader("BaseData/numForce.txt", encoding: System.Text.Encoding.UTF8)) {
                countForce = int.Parse(srMain.ReadLine());
                mainIndexForce = countForce;
            }

            for (int i = 0; i < countForce; i++) {
                Force temp_f = new Force("", 0, 0);
                using (var sr = new StreamReader("BaseData/force/" + i.ToString() + ".txt", encoding: System.Text.Encoding.UTF8)) {
                    temp_f.ForceMain = sr.ReadLine();
                    temp_f.Commentar = sr.ReadLine();
                    temp_f.index = int.Parse(sr.ReadLine());
                    temp_f.countWin = int.Parse(sr.ReadLine());
                    temp_f.countProblem = int.Parse(sr.ReadLine());
                    temp_f.idHuman = long.Parse(sr.ReadLine());
                }

                all_Force.Add(temp_f);
            }

            CheckForce();
        }
    }
}
