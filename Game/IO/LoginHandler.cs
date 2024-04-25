﻿using Silvarea.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silvarea.Game.IO
{
    internal class LoginHandler //Yeah I know it needs a different name, not sure what to call it
    {

        public static Packet Login(Session session, string username, string password, Boolean isLowMemory)
        {
            //lotta messy logic to go here!!!
            //Remember that 18 for CurrentState possible in login decoder? If it is 18, send back 15 as successful returncode, this stops the chatbox from getting cleared :)
            LoginReturnCode returnCode = LoginReturnCode.SUCCESS;
            return GenerateReply(session, returnCode);//loginReply;//login reply packet
        }

        public static Packet GenerateReply(Session session, LoginReturnCode returnCode)
        {
            Packet loginReply = new Packet(new MemoryStream());
            loginReply.p1((int) returnCode);
            switch (returnCode)
            {
                case LoginReturnCode.SUCCESS:
                    loginReply.p1(0);//player rights; 0 = player, 1 = pmod, 2 = jmod
                    loginReply.p1(0);//bot flag, makes the client send mouse tracking packets back to server if set to 1; incoming packet opcode = 94
                    loginReply.p2(0);//player index
                    loginReply.p1(1);//1 = members, 0 = free
                    break;
                case LoginReturnCode.TRANSFER_DELAY:
                    loginReply.p1(30);//number of seconds before logging in
                    break;
            }
            return loginReply;
        }

        public enum LoginReturnCode //Up to date for #410
        {
            CONNECTION_TIMED_OUT = -3,//We don't send this typically, it's an internal code for the client.
            ERROR_CONNECTING = -2,//We don't send this typically, it's an internal code for the client.
            RETRY_WITH_COUNT = -1,//wait 2000ms and tries again with attempt count displayed
            RESEND_INFO = 0,//requests login handshake again? Exchanges session keys, player name, password
            RETRY = 1,//wait 2000ms and tries again
            SUCCESS = 2,//Successful normal login - <byte>returncode, <byte>unknown, <byte> unknown, <short> playerIndex, <byte>membersFlag
            INVALID_CREDENTIALS = 3,
            ACCOUNT_DISABLED = 4,
            ALREADY_LOGGED_IN = 5,
            GAME_UPDATED = 6,
            WORLD_FULL = 7,
            LOGIN_SERVER_OFFLINE = 8,
            LOGIN_LIMIT_EXCEEDED = 9,//too many connections from one IP (one account logged in, different account logs in from same IP)
            BAD_SESSION_ID = 10,
            REJECTED_SESSION = 11,//We suspect someone knows your password. Press Change Password on front page.
            MEMBERS_ONLY = 12,
            UNABLE_TO_COMPLETE = 13,
            GAME_UPDATING = 14,
            RECONNECTING = 15,//This one is cool, if the client drops connection it tries again. If successful, we send this returncode which stops the client from clearing the chatbox - sends back 4 bytes after, first byte is 2 or 3 depending on something in client, next is a g3() that should be a 0.
            LOGIN_ATTEMPTS_EXCEEDED = 16,
            MOVE_TO_FREE_AREA = 17,
            ACCOUNT_STOLEN = 18,
            INVALID_LOGIN_SERVER = 20,
            TRANSFER_DELAY = 21, //You have only just left another world. Your profile will be transferred in: <time> seconds. - send single byte with time in seconds
            MALFORMED_LOGIN_PACKET = 22,
            NO_REPLY_FROM_LOGINSERVER = 23,//resets login stage in client? Login failed and try again? Try this one! No extra bytes required
            ERROR_LOADING_PROFILE = 24,
            ADDRESS_BLOCKED = 26//This computer's address has been blocked as it was used to break our rules.
        }

    }
}