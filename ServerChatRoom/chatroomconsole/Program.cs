using chatbusiness;
using chatbusiness.Core;
using chatcommon.Classes;
using chatcommon.Entities;
using chatcommon.Interfaces;
using chatgateway;
using chatgateway.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using chatroomconsole.Classes;
using System.Configuration;

namespace chatroomconsole
{
    class Program
    {
        public static BusinessLogic Bl;
        public static DataAccess Dal;
        

        static void Main(string[] args)
        {
            initialize();
            Server Server = new Server(Bl);
            //Console.Read();
        }

        public static void initialize()
        {
            Dal = new DataAccess(
                                new DiscussionGateway(),
                                new UserGateway(),
                                new MessageGateway(),
                                new User_discussionGateway(),
                                new SecurityGateway());

            BLSecurity BLSecurity = new BLSecurity(Dal);
            Dal.SetUserCredential( new NotifyTaskCompletion<User>( BLSecurity.AuthenticateUser(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"], false)).Task.Result);
            Bl = new BusinessLogic(
                                new BLDiscussion(Dal),
                                new BLUser(Dal),
                                new BLMessage(Dal),
                                new BLUser_discussion(Dal),
                                BLSecurity);  
        }

        
    }
}
