using System;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;

using Newtonsoft.Json;


namespace VPN_Status_Checker

{
    public partial class Home : System.Web.UI.Page

    {
        protected void Page_Load(object sender, EventArgs e)
        {

            int dropdownValue = 0; // Auto-run timer interval

            // Catch report download query string
            if (Request["download"] == "true") { downloadReport(); }

            // Auto-run tests at specified interval
            if (chkAutoRun.Checked)
            {
                try
                {
                     dropdownValue = Int32.Parse(dlAutoInterval.SelectedValue);

                } catch {

                     // Reset to default value if value not int
                     dropdownValue = 360; // 6-hours
                }

                tmrAutoRun.Interval = dropdownValue * 60 * 1000;
                tmrAutoRun.Enabled = true;
            }
        }

    protected void btnRunTests_Click(object sender, EventArgs e)
        {

            // Start runner
            runTests();

        }

    public void runTests()
        {

            // Run before task
            before();

            // Run tests

            try

            {
                // Test if any servers are down
                testDownServers();

                // Check if VPN network fulfills minimum network requirments (> 0 secure core server, > 0 basic server and > 0 free server
                testMinimumActiveServers();

                // Test for servers with high load
                testServerLoad();

                // Test if servers are missing info and find their status
                testServersInvalidInfo();

                // Add date to report
                addTestDate(4);

                // Contact network team if there is a down server
                if (chkAutoAlert.Checked && Globals.downServers > 0)
                {
                    sendMail("VPN Automatic Status Report " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                }

            }

            catch (Exception ex)
            {

                lblError.Text += "<br /><br />Error during test:<br /> " + ex.Message;

            }

        }

        public void testDownServers()

        {

            Globals.downServers = 0;
            lblDownServers.Text = "Server status<br />----------------------------";

            foreach (var servers in Globals.account.LogicalServers)
            {
                try
                  {
                        foreach (var server in servers.Servers)
                        {

                            // Look for down server

                            if (server.Status == 0)

                            {

                                lblDownServers.Text += "<br /><b>Server down in: " + servers.EntryCountry + " - Server name: " + servers.Name + "</b>" +
                                "<br />Entry IP: " + server.EntryIP + " Exit IP: " + server.ExitIP + " | Domain: " + server.Domain + " Status code: " + server.Status;

                                // Check server response with ping; add additional info to the log

                                Ping ping = new Ping();
                                PingReply pingResult = ping.Send(server.EntryIP);

                                if (pingResult.Status.ToString() == Globals.timeOut)
                                {
                                    lblDownServers.Text += " | Pinged: " + server.EntryIP + " ping status: " + "timed out";
                                }
                                else if (pingResult.Status.ToString() == Globals.success)
                                {
                                    lblDownServers.Text += " | Pinged: " + server.EntryIP + " ping status: " + "received reply";
                                }

                                // Ping Exit IP if different from Entry IP

                                if (server.EntryIP != server.ExitIP)
                                {

                                    pingResult = ping.Send(server.ExitIP);

                                    if (pingResult.Status.ToString() == Globals.timeOut)
                                    {
                                        lblDownServers.Text += " | Pinged: " + server.ExitIP + " ping status: " + "timed out";
                                    }
                                    else if (pingResult.Status.ToString() == Globals.success)
                                    {
                                        lblDownServers.Text += " | Pinged: " + server.ExitIP + " ping status: " + "received reply";
                                    }
                                }

                            Globals.downServers++;

                        }

                    }

                }
                catch (Exception ex)
                {
                    // Throw all available server info in case configuration detail is missing
                    lblDownServers.Text += "<br /><b>Detected down server</b> - missing object data: " + servers.ToString() + "<br />" + ex;
                    Globals.downServers++; // trigger warning to NOC
                }
            }

            // Test results

            if (Globals.downServers > 0)
            {
                lblDownServers.Text += @"<br /><br />Test status: <b style=""color: red; "">Fail</b>";

            } else
            {
                if (Globals.downServers == 0) {
                    lblDownServers.Text += "<br /><br />All servers are active.";
                }
                lblDownServers.Text += @"<br />Test status: <b style=""color: green; "">Success</b>";
            }

            lblDownServers.Text += "<br />----------------------------";

            // Add data to report

            Globals.report += lblDownServers.Text + "<br />";
            Globals.testsRun ++;
        }

        public void testMinimumActiveServers()
        {
            int basicServers = 0; // 0
            int secureCoreServers = 0; // 1
            int torServers = 0; // 2
            int p2pServers = 0 ; // 4
            int freeServers = 0; // free
            int plusServers = 0; // plus

            lblActiveServers.Text += "Active servers<br />----------------------------";

            foreach (var servers in Globals.account.LogicalServers)

            {

                try {

                    // Skip offline servers

                    if (servers.Status == 1)
                    {
                        switch (servers.Features)
                        {
                            case 0:
                                basicServers++;
                                break;

                            case 1:
                                secureCoreServers++;
                                break;

                            case 2:
                                torServers++;
                                break;

                            case 4:
                                p2pServers++;
                                break;
                        }

                        // Check for free servers

                        if (servers.Domain.Contains("-free"))

                        {
                            freeServers++;
                        }

                        // Check for plus servers

                        if (servers.Tier >= 2) // Larger or equal to 2 was specified in the QA Task. API response seems to be always == 2 for Plus servers
                        {
                            plusServers++;
                        }
                    }
                }
                
                catch (Exception ex)
                {
                    lblActiveServers.Text += "<br /><b>Server missing object data: " + servers.ToString() + "<br />" + ex;
                }

            }

            lblActiveServers.Text += "<br />Basic servers: " + basicServers + "<br />Secure core servers: " + secureCoreServers +
            "<br />Tor servers: " + torServers + "<br />P2P servers: " + p2pServers + "<br />Free servers: " + freeServers + "<br />Plus servers: "
            + plusServers + "<br />Total number of logical servers: " + (basicServers + secureCoreServers + torServers + p2pServers);

            // Test results

            if (freeServers > 0 && secureCoreServers > 0 && freeServers > 0)
                {
                    lblActiveServers.Text += @"<br /><br />VPN network status: <b style=""color: green; "">Functional</b>";

                }
                else
                {
                    lblActiveServers.Text += @"<br /><br />VPN network status: <b style=""color: red; "">Non-Functional</b>";
                }

                lblActiveServers.Text += "<br />----------------------------";

                // Add data to report
                Globals.report += lblActiveServers.Text + "<br />";
                Globals.testsRun++;
        }

        public void testServerLoad()

        {

            int highLoadServers = 0;

            lblHighLoadServers.Text += "High load servers<br />----------------------------";

            foreach (var servers in Globals.account.LogicalServers)
            {

                try

                {
                    // Skip offline servers

                    if (servers.Status == 1)

                    {

                        if (servers.Load >= 90)
                        {
                            lblHighLoadServers.Text += "<br /><b>" + servers.Name + "</b>" + " - domain: " + servers.Domain + " | load index: <b>" + servers.Load + "</b>";

                            highLoadServers++;

                        }

                    }
                }

                catch (Exception ex)
                {
                    lblHighLoadServers.Text += "<br /><b>Server missing object data: " + servers.ToString() + "<br />" + ex;
                }
                
            }

            // Test results

            if (highLoadServers > 0)
            {
                lblHighLoadServers.Text += @"<br /><br />Network load status: <b style=""color: red; "">Detected servers with high load levels</b>";
            } else
            {
                lblHighLoadServers.Text += @"<br /><br />Network load status: <b style=""color: green; "">OK</b>";
            }

            lblHighLoadServers.Text += "<br />----------------------------";

            // Add data to report
            Globals.report += lblHighLoadServers.Text + "<br />";
            Globals.testsRun++;

        }

        public void testServersInvalidInfo()
        {

            int serversInvalidInfo = 0;

            lblInvalidInfo.Text += "Misconfigured servers<br />----------------------------";

            foreach (var servers in Globals.account.LogicalServers)

                try {

                    {

                        // Check for unexpected server status code

                        if (servers.Status != 0 && servers.Status != 1)

                        {

                            lblInvalidInfo.Text += "<br />" + servers.Name + "</b>" + "- Status code: " + servers.Status + " | Status code must be 0 or 1";
                            serversInvalidInfo++;

                        }

                        foreach (var server in servers.Servers)
                        {

                            // Check for unexpected IP structure

                            IPAddress IP;

                            if (!IPAddress.TryParse(server.EntryIP, out IP))
                            {
                                lblInvalidInfo.Text += "<br />" + servers.Name + "</b>" + " - Entry IP: " + server.EntryIP + " | non-valid IPv4 format";
                                serversInvalidInfo++;
                            }

                            if (server.EntryIP != server.ExitIP)
                            {
                                if (!IPAddress.TryParse(server.ExitIP, out IP))
                                {
                                    lblInvalidInfo.Text += "<br />" + servers.Name + "</b>" + " - Exit IP: " + server.ExitIP + " | non-valid IPv4 format";
                                    serversInvalidInfo++;
                                }
                            }

                        }

                    } 

            }
                catch (Exception ex)
                {
                    lblInvalidInfo.Text += "<br /><b>Server missing object data: " + servers.ToString() + "<br />" + ex;
                }

            // Test results

            if (serversInvalidInfo > 0)
            {
                lblInvalidInfo.Text += @"<br /><br />Test status: <b style=""color: red; "">Detected misconfigured servers</b>";
            } else
            {
                lblInvalidInfo.Text += "<br /><br />All servers are properly configured.";
                lblInvalidInfo.Text += @"<br />Test status: <b style=""color: green; "">OK</b>";
            }

            lblInvalidInfo.Text += "<br />----------------------------";

            // Add data to report
            Globals.report += lblInvalidInfo.Text + "<br />";
            Globals.testsRun++;

        }

        public void testHostNotResponding()
        {

            Globals.domainsNotResponding = 0;

            lblDownServers.Text += "Non-responding servers<br />----------------------------";

            foreach (var servers in Globals.account.LogicalServers)
            { 
                try
                {
                    foreach (var server in servers.Servers)
                    {

                        // Check for non-responding domains

                        Ping ping = new Ping();
                        PingReply pingResult = ping.Send(server.EntryIP);

                        if (pingResult.Status.ToString() == Globals.timeOut)
                        {
                            lblDownServers.Text += "<br /><b>" + servers.Name + " - " + server.EntryIP + "</b> | ping status: " + "timed out";
                            Globals.domainsNotResponding++;
                        }

                        // Ping Exit IP if different from Entry IP

                        if (server.EntryIP != server.ExitIP)
                        {

                            pingResult = ping.Send(server.ExitIP);

                            if (pingResult.Status.ToString() == Globals.timeOut)
                            {
                                lblDownServers.Text += "<br /><b>" + servers.Name + " - " + server.ExitIP + "</b> | ping status: " + "timed out";
                                Globals.domainsNotResponding++;

                            }
                        }
                }

                } catch (Exception ex)
                {

                    lblDownServers.Text += "<br /><b>Server missing object data: " + servers.ToString() + "<br />" + ex;
                    Globals.domainsNotResponding++; // trigger warning to NOC

                }
            }

            // Test results

            if (Globals.domainsNotResponding > 0)
            {
                lblDownServers.Text += @"<br /><br />Test status: <b style=""color: red; "">Detected non-responding IPs</b>";
            } else
            {
                lblDownServers.Text += "><br /><br />All servers are responding to the ping command.";
                lblDownServers.Text += @"<br />Test status: <b style=""color: green; "">OK</b>";
            }

            lblDownServers.Text += "<br />----------------------------";

            // Add data to report
            Globals.report += lblDownServers.Text;
            Globals.testsRun++;

        }

        protected void btnEmail_Click(object sender, EventArgs e)
        {
            sendMail("VPN Server Status Report " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));

        }

        public void sendMail(string emailSubject)

        {

            if (Globals.report != null) { 

            // Note: Need to set SMTP login credentials and alert destination in order for the function to work

            try
            {

                 String emailFrom = "test@gmail.com";  // Set alert from email field
                 String emailTo = "example@gmail.com"; // Set email alert destination
                 SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587); // Set SMTP server and port

                smtpClient.UseDefaultCredentials = false;

                smtpClient.Credentials = new System.Net.NetworkCredential("test@gmail.com", "password"); // Set SMTP login credentials
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.EnableSsl = true;
                MailMessage mail = new MailMessage();

                // Set email body
                mail.From = new MailAddress(emailFrom, "VPN Status Checker");
                mail.To.Add(new MailAddress(emailTo));
                mail.Subject = emailSubject;

                // Clear HTML from message body
                String sb = Server.HtmlDecode(Globals.report);
                sb = sb.Replace("<br />", Environment.NewLine);
                sb = sb.Replace("<b>", "");
                sb = sb.Replace("</b>", "");
                sb = sb.Replace(@"<b style=""color: red; "">", "");
                sb = sb.Replace(@"<b style=""color: green; "">", "");

                mail.Body = Server.HtmlDecode(sb);

                smtpClient.Send(mail);

                lblEmailAlert.Text = "Alert to NOC has been sent.";

            }

            catch (Exception ex)
            {
                    //throw ex;
                    Response.Write(@"<script>alert('Please configure SMTP credentials before using the send alert function.');</script>");
                }
          } else
            {
                Response.Write(@"<script>alert('Please generate a report by clicking on the ""Run Tests"" or ""Find Down Servers"" buttons.');</script>");
            }
        }

        protected void btnDownloadReport_Click(object sender, EventArgs e)
        {

            // Download network status report

            // Make sure report is not empty
            if (Globals.report != null) { 
            Response.Redirect(Request.Url + "?download=true");
            } else
            {
                // Alert user if report is not available
                Response.Write(@"<script>alert('Please generate a report by clicking on the ""Run Tests"" or ""Find Down Servers"" buttons.');</script>");
            }

        }

        private void downloadReport()
        {

            String today = util.currentDateTime();

            // Output report as HTML
            try
            {
                Response.ContentType = "text/plain";
                Response.AddHeader("content-disposition", "attachment;filename=vpn_server_report" + util.currentDateTime() + ".html");
                Response.Write(Globals.report);
                Response.Flush();
                Response.End();
            } catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void tmrAutoRun_Tick(object sender, EventArgs e)
        {
            runTests();

        }

        public void clearTestData()
        {

            Globals.testsRun = 0;
            Globals.report = null;

            lblDate.Text = null;
            lblActiveServers.Text = null;
            lblDownServers.Text = null;
            lblError.Text = null;
            lblHighLoadServers.Text = null;
            lblInvalidInfo.Text = null;

        }

        protected void btnPing_Click(object sender, EventArgs e)
        {

            // Run before task
            before();

            try { 

            // Ping the listed domains for any non-responding host

            testHostNotResponding();

            // Add date to report
            addTestDate(1);

            // Contact network team if there is a down server
            if (chkAutoAlert.Checked && Globals.domainsNotResponding > 0)
            {
                sendMail("VPN Automatic Status Report " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            }

            }
            catch (Exception ex)
            {

                lblError.Text += "<br /><br />Error during test:<br /> " + ex.Message;

            }
}

        private void before()
        {

            // Run cleanup
            clearTestData();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            // Get API response
            String result = util.getResponse("https://api.protonmail.ch/vpn/logicals");

            // Load JSON objects
            try
            {
                Globals.account = JsonConvert.DeserializeObject<RootObject>(result);
            }

            catch (Exception ex)
            {
                // Add to the log servers that are missing info

                lblError.Text = "Server(s) missing info in JSON response:<br />" + ex.Message;

            }
        }

        private void addTestDate(int maxRuns)
        {
            lblDate.Text = "<br />Test date: " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Test runs: " + Globals.testsRun + "/" + maxRuns;
            lblDate.Text += "<br />----------------------------";
            Globals.report += lblDate.Text + "<br />";
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            clearTestData();
        }
      }
    }
