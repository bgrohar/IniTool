/* 
 *  IniTool - for Bethesda's .ini config files
 *
 */
using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Controls;

namespace IniTool
{

    public partial class IniTool : Form
    {
        int highestPercentageReached = 0;
        int percentComplete = 0;
        bool isRunning;

        // SORT vars
        //bool includeTitles;

        //int sortMode;
        string masterFilePath;
        string unsortedFilePath;
        string[] files;
        string[] unsortedFileLines;
        string[] masterFileLines;
        string[] sortedLines;
        public BackgroundWorker backgroundWorker1;

        // IDENT vars
        List<string> fileSelection = new List<string>();
        string[] identicalLines;

        // COMMON vars
        string[] commonSettings;

        // DIFF vars
        string[] differentSettings;

        public IniTool()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           // FormTest();

            string ver = "2.3";
            this.Size = new Size(800, 500);
            this.MinimumSize = new Size(800, 500);
            this.MaximumSize = new Size(800, 500);
            //includeTitles = false;

            Panel1.Hide();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);


            // ConsoleBox
            ConsoleBox1.Show();
            ConsoleBox1.WordWrap = true;
            ConsoleBox1.ReadOnly = true;
            ConsoleBox1.ScrollBars = ScrollBars.Both;
            ConsoleBox1.AppendText(String.Format(">>> IniTool ver. {0} <<<", ver));
            ConsoleBox1.AppendText(Environment.NewLine);

            // Populate main dropdown
            DropdownBox1.Items.Add("SORT");
            DropdownBox1.Items.Add("Identical lines");
            DropdownBox1.Items.Add("Common settings");
            DropdownBox1.Items.Add("Difference");
            DropdownBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            // SORT menu
            Label2.Hide();

            // Populate SORT.filedropdown menu
            files = Directory.GetFiles(@"..\..\..\Resources");
            List<string> tmp = new List<string>();
            foreach (string file in files) {
                tmp.Append(Path.GetFullPath(file));
                FileDropdown1.Items.Add(Path.GetFileName(file));
            }
            tmp.ToArray().CopyTo(files,0);
            Console.WriteLine("New lines is:" + files.Length.ToString());
            FileDropdown1.DropDownStyle = ComboBoxStyle.DropDownList;
            FileDropdown1.Hide();

            // SORT.open file for sorting
            Label3.Hide();
            OpenFileButton1.AllowDrop = true;
            //OpenFileButton1.Hide();
            OpenFileButton1.Show();
            Label4.Text = "";
            // SORT button
            CheckBox2.Hide();
            CheckBox3.Hide();
            CheckBox4.Hide();
            SortButton1.Hide();

            // IDENTICAL LINES MENU
            Label5.Hide();
            OpenFileButton2.Hide();
            ClearSelectionButton1.Hide();
            SelectionBox1.WordWrap = true;
            SelectionBox1.ReadOnly = true;
            SelectionBox1.ScrollBars = ScrollBars.Both;
            SelectionBox1.Hide();
            Label6.Hide();
            Panel2.Hide();

            // COMMON SETTINGS MENU
            Label7.Hide();
            Label8.Hide();
            Label9.Hide();
            OpenFileButton2.Hide();
            ClearSelectionButton2.Hide();
            SelectionBox2.Hide();
            CompareFilesButton2.Hide();
            Panel3.Hide();

            // DIFF MENU
            Panel4.Hide();


            // Switch to first mode automatically
            ToggleSORT(true);
            DropdownBox1.SelectedIndex = 0;
            
        }

        private void ResetVariables() {

            Console.WriteLine("-> VARIABLES RESET !");

            // Reset text boxes here as well
            Label4.Text = "";
            FileDropdown1.Text = "";
            FileDropdown1.SelectedIndex = -1;


            // backgroundworker1 vars
            highestPercentageReached = 0;
            percentComplete = 0;
            isRunning = false;

            // IDENT vars
            UpdateSelectionBox(SelectionBox1);

            // COMMON vars
            UpdateSelectionBox(SelectionBox2);
            // fileSelection = new List<string>();

            // DIFF vars
            UpdateSelectionBox(SelectionBox3);

            
        }

        private void HandleDropdown()
        {
            ResetVariables();
            string var = DropdownBox1.Text;
            switch (var) {
                case "SORT":
                    ToggleSORT(true);
                    ToggleIdent(false);
                    ToggleDiff(false);
                    ToggleCommon(false);
                    break;
                case "Identical lines":
                    ToggleSORT(false);
                    ToggleIdent(true);
                    ToggleDiff(false);
                    ToggleCommon(false);
                    break;
                case "Difference":
                    ToggleSORT(false);
                    ToggleIdent(false);
                    ToggleDiff(true);
                    ToggleCommon(false);
                    break;
                case "Common settings":
                    ToggleSORT(false);
                    ToggleIdent(false);
                    ToggleDiff(false);
                    ToggleCommon(true);
                    break;
                case "Only in first":
                    ToggleSORT(false);
                    ToggleIdent(false);
                    ToggleDiff(false);
                    ToggleCommon(false);
                    break;
                default:
                    break;
            }
        }

        private void ToggleSORT(bool shouldShow)
        {
            ResetVariables();
            ProgressBar1.Hide();
            CancelButton1.Hide();
            SortButton1.Hide();

            if (shouldShow){

                if (backgroundWorker1 == null) { 
                    Console.WriteLine("-> Creating backgroundWorker1.");
                    backgroundWorker1 = new BackgroundWorker();
                    backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
                    backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
                    backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
                }
                backgroundWorker1.WorkerReportsProgress = true;
                backgroundWorker1.WorkerSupportsCancellation = true;


                Panel1.Show();
                Panel1.Location = new Point(12,45);
                Panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left;

                Label2.Hide();
                Label3.Show();
                FileDropdown1.Hide();
                CheckBox2.Hide();
                CheckBox3.Hide();
                CheckBox4.Hide();

            }else{   
                Panel1.Hide();

                Console.WriteLine("-> Deleting bgWorker1 events...");
                backgroundWorker1.DoWork -= new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
                backgroundWorker1.ProgressChanged -= new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
                backgroundWorker1.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            }
        }

        private void ToggleIdent(bool shouldShow) {
            if (shouldShow)
            {
                Panel2.Show();

                Panel2.Location = new Point(12, 45);
                Panel2.Anchor = AnchorStyles.Top;
                Panel2.Anchor = AnchorStyles.Left;

                Label5.Show();
                Label6.Show();
                OpenFileButton2.Show();
                ClearSelectionButton1.Show();
                SelectionBox1.Show();
                CompareFilesButton1.Hide();
                CheckBox1.Show();
            }
            else{ Panel2.Hide(); }
        }

        private void ToggleCommon(bool shouldShow)
        {
            if (shouldShow)
            {
                Panel3.Show();

                Panel3.Location = new Point(12, 45);
                Panel3.Anchor = AnchorStyles.Top;
                Panel3.Anchor = AnchorStyles.Left;

                Label7.Show();
                Label8.Show();
                Label9.Show();
                ClearSelectionButton2.Show();
                OpenFileButton3.Show();
                SelectionBox2.Show();
                CompareFilesButton2.Hide();
            }
            else { Panel3.Hide(); }
        }

        private void ToggleDiff(bool shouldShow)
        {
            if (shouldShow)
            {
                Panel4.Show();

                Panel4.Location = new Point(12, 45);
                Panel4.Anchor = AnchorStyles.Top;
                Panel4.Anchor = AnchorStyles.Left;

                label10.Show();
                label11.Show();
                label12.Show();
                ClearSelectionButton3.Show();
                OpenFileButton4.Show();
                SelectionBox3.Show();
                CompareFilesButton3.Hide();
            }
            else { Panel4.Hide(); }
        }

        public void dumpToFile(List<string> items, string filename)
        {
            StreamWriter sw = null;
            try
            {
                string path = String.Format(@"F:\Desktop\iniTool test site\debug\{0}", filename);
                sw = new StreamWriter(path, false);
                foreach(string s in items) sw.WriteLine(s);
                Console.WriteLine(String.Format("-> Dumping to file: {0}", filename));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Error: File not found.");
            }
            catch (IOException)
            {
                Console.WriteLine("Error: IO");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
        }

        private string[] Formatter(string line) {
            string[] ret = new string[2];
            string setting = line.Remove(line.IndexOf('='));
            string value = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 1); 
            ret[0] = setting;
            ret[1] = value;
            return ret;
        }

        // MasterSort currently in backGroundWorker1
        private string[] MasterSort(string[] lines)
        {
            int lineCount = 0;
            List<string> list = new List<string>(lines);
            List<string> master = new List<string>(masterFileLines);
            List<string> sorted = new List<string>();
            foreach (string m in master) {
                lineCount += 1;
                if (string.IsNullOrEmpty(m)) continue;

                // If titles not allowed, skip lines with title
                //if (!includeTitles)  if (m.IndexOf('[') != -1) continue;

                if (sorted.Contains(m)) continue;

                foreach (string l in list) {
                    lineCount += 1;
                    if(string.IsNullOrEmpty(l)) continue;

                    // If titles not allowed, skip lines with title
                    //if (!includeTitles) if (l.IndexOf('[') != -1) continue;

                    if (sorted.Contains(l)) continue;

                    if (m.Equals(l)) {
                            sorted.Add(l);
                           //  Console.WriteLine("Appended: " + l);
                            continue;
                    }
                    //if (l.IndexOf('[') == -1 && m.IndexOf('[') == -1)
                    //{
                        string mset = Formatter(m)[0];
                        string lset = Formatter(l)[0];
                        if (mset.Equals(lset) && !sorted.Contains(l))
                        {
                            sorted.Add(l);                         
                        }
                    //}
                }
            }
            ConsoleBox1.AppendText(String.Format("Processed {0} lines. Found {1} lines.", lineCount, sorted.Count));
            ConsoleBox1.AppendText(Environment.NewLine);

            if (CheckBox2.Checked){
                return sorted.ToArray();
            } else{
                return RemoveTitles(sorted.ToArray());
            }
           // return sorted.ToArray();
        }

        private string[] AlphaSort(string[] lines) {
            string[] tmp = lines;
            List<string> returnValue = new List<string>();

            Array.Sort(tmp);
            
            foreach (string m in tmp) { 
                if (string.IsNullOrEmpty(m)) continue;

                // If titles not allowed, skip lines with title
                if (!CheckBox2.Checked)  if (m.IndexOf('[') != -1) continue;
                returnValue.Add(m);
            }
            
            ConsoleBox1.AppendText(String.Format("Processed {0} lines.", lines.Length));
            ConsoleBox1.AppendText(Environment.NewLine);
            return returnValue.ToArray();
        }

        private string[] FindIdenticalLines() {
            List<List<string>> allFiles = new List<List<string>>();
            int lineCount = 0;
            foreach (string file in fileSelection) {
                // Read each file into a list
                List<string> fileLines = new List<string>(ReadFile(file, false));
                allFiles.Add(fileLines);
                lineCount += fileLines.Count;
            }
            var result = allFiles
                .Skip(1)
                .Aggregate(
                    new HashSet<string>(allFiles.First()),
                    (h, e) => { h.IntersectWith(e); return h; }
                );

            ConsoleBox1.AppendText(String.Format("Processed {0} lines. Found {1} lines.", lineCount, result.Count));
            ConsoleBox1.AppendText(Environment.NewLine);
            ConsoleBox1.AppendText(String.Format("includeTitles was: {0}", CheckBox2.Checked.ToString()));
            ConsoleBox1.AppendText(Environment.NewLine);

            if (CheckBox2.Checked) {
                return result.ToArray();
            } else {
                return RemoveTitles(result.ToArray());
            }
            
        }

        private string[] FindCommon(List<string> fs, bool mode)
        {
            // mode = false .... looks for common setting names only
            // mode = true .... looks for common setting names and different values
           
            // Files can be modified without requiring reload here
            List<string> result = new List<string>();
            List<Dictionary<string, string>> dictList = new List<Dictionary<string, string>>();
            Dictionary<string, List<string>> mainDict = new Dictionary<string, List<string>>();
            
            // Read all setting names from first file
            List<string> firstFile = new List<string>(
                RemoveTitles(
                    ReadFile(
                        fs.First(),false)));

            foreach (string ln in firstFile)
            {
                // Extract setting name, value with Formatter
                string k = Formatter(ln)[0];
                string v = Formatter(ln)[1];

                // mainDict is: { key, values=[] }
                if (!mainDict.ContainsKey(k))
                {
                    mainDict.Add(k, new List<String>());
                }
                mainDict[k].Add(v); 
            }

            // Read all other files
            foreach (var file in fs.Skip(1))
            {
                List<string> temp = new List<string>(RemoveTitles(ReadFile(file,false))); // Read to list
                foreach (string val in temp)
                {
                    string k = Formatter(val)[0];  // Get key from non-first file
                    if (mainDict.ContainsKey(k))
                    {
                        if (mode) {
                            //mainDict[k].Add(Formatter(val)[1]);
                            mainDict[k].Add("0"); // The value does not matter here
                            continue;
                        }
                        if (!mainDict[k].Contains(Formatter(val)[1]))
                        {
                            mainDict[k].Add(Formatter(val)[1]);
                        }
                    }
                    else {
                        Console.WriteLine("mainDict does not contain key: "+k.ToString());
                    }
                }
            }

            foreach (KeyValuePair<string, List<string>> kvp in mainDict)
            {
                if (kvp.Value.Count > 1)
                {   // Unroll list into a string and comment whole line out
                    result.Add(String.Format("{0}=({1})", kvp.Key, string.Join(",  ", kvp.Value.ToArray())));
                }

            }
            return result.ToArray();
        }

        private string[] EXPFindCommon(List<string> fs, bool mode) {
            // mode = false .... looks for common setting names only
            // mode = true .... looks for common setting names and different values

            List<string> mainList = new List<string>();

            // Read all setting names from first file
            List<string> firstFile = new List<string>(
                RemoveTitles(
                    ReadFile(
                        fs.First(), false)));

            foreach (string ln in firstFile)
            {
                // Extract setting name, value with Formatter
               mainList.Add(Formatter(ln)[0]);
            }

            mainList = new List<string>(mainList.Distinct());

            
            // Read all other files
            foreach (var file in fs.Skip(1))
            {
                List<string> temp = new List<string>(RemoveTitles(ReadFile(file,false)).Distinct()); // Read to list
                foreach (string val in temp)
                {
                    mainList.Add(Formatter(val)[0]);
                }
            }

            var query = mainList.GroupBy(x => x)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();

            return query.ToArray();
        }

        private string[] FindDiffSettings() { 
            List<string> result = new List<string>();
            List<string> all = new List<string>();
            List<string> commonTemp = new List<string>();

            //commonSettings = FindCommon(fileSelection,false);
            commonSettings = EXPFindCommon(fileSelection,false);
            Console.WriteLine("fileSelection:" + fileSelection.Count);

            foreach (string s in commonSettings) {
           //     commonTemp.Add(Formatter(s)[0]);
                Console.WriteLine("commonSettings is: " + s);
            }

            foreach (var file in fileSelection){ // No skipping here
                List<string> nfile = new List<string>(RemoveTitles(ReadFile(file,false))).ToList();
                foreach(string cl in nfile){
                    all.Add(Formatter(cl)[0]);
                    Console.WriteLine("Added line to all: " + cl + "OG: " + Formatter(cl)[0]);
                }
               // all = new List<string>(all).Distinct().ToList<string>();
            }

            var list1 = all.Except(commonSettings).ToList();
            var list2 = commonSettings.Except(all).ToList();
            result = list1.Concat(list2).ToList();
            OutputForm(result.ToArray());
            return result.ToArray();
        }

        private string[] RemoveTitles(string[] source) {
            List<string> tmp = new List<String>();
            foreach (string ln in source) {
                if (string.IsNullOrEmpty(ln)) { continue; }
                if (ln.IndexOf('[') == -1 && ln.IndexOf('=') > -1) {
                    tmp.Add(ln);
                }   
            }
            return tmp.ToArray();
        }

        private string[] ReadFile(string path, bool displayMsg) {
            string[] lines;
            if (File.Exists(@path)){
                lines = File.ReadAllLines(path);
                if (displayMsg) {
                    ConsoleBox1.AppendText(String.Format("Loaded file ! -> {0}, ({1} lines)",path,lines.Length));
                    ConsoleBox1.AppendText(Environment.NewLine);
                }
            }
            else{
                ConsoleBox1.AppendText("ERROR: File not found...");
                ConsoleBox1.AppendText(Environment.NewLine);
                throw new FileNotFoundException("File not found...");
            }
            return lines;
        }

        private void FormTest() {
            var myScreen = Screen.FromControl(this);
            var mySecondScreen = Screen.AllScreens.FirstOrDefault(s => !s.Equals(myScreen)) ?? myScreen;

            Form Form2 = new Form();
            Form2.Show();
            Form2.Left = mySecondScreen.Bounds.Left;
            Form2.Top = mySecondScreen.Bounds.Top;
            Form2.StartPosition = FormStartPosition.Manual;

            Form2.Size = new Size(300, 800);
            Form2.MinimumSize = new Size(300, 800);
            Form2.MaximumSize = new Size(300, 800);

            System.Windows.Forms.TextBox tbox = new System.Windows.Forms.TextBox();
            tbox.Size = new Size(280, 760);
            tbox.Multiline = true;
            tbox.WordWrap = true;
            tbox.ReadOnly = true;
            tbox.ScrollBars = ScrollBars.Both;
            tbox.Font = new Font("Arial", 12, FontStyle.Bold);
            Form2.Controls.Add(tbox);
        }

        private void OutputForm(string[] linesToDisplay) {
            var myScreen = Screen.FromControl(this);
            var mySecondScreen = Screen.AllScreens.FirstOrDefault(s => !s.Equals(myScreen)) ?? myScreen;

            Form Form2 = new Form();
            Form2.Show();
            Form2.Left = mySecondScreen.Bounds.Left;
            Form2.Top = mySecondScreen.Bounds.Top;
            Form2.StartPosition = FormStartPosition.Manual;

            Form2.Size = new Size(600, 800);
            Form2.MinimumSize = new Size(300, 800);
            Form2.MaximumSize = new Size(2560, 800);
            Form2.AutoSize = true;
            // Form2.Location = new Point(150, 150);
            System.Windows.Forms.TextBox tbox = new System.Windows.Forms.TextBox();
            //tbox.Size = new Size(780, 750);
            tbox.Size = new Size(1200, 750);
            tbox.MaximumSize = new Size(2560, 1440);
            tbox.Multiline = true;
            tbox.WordWrap = false;
            tbox.ReadOnly = true;
            tbox.ScrollBars = ScrollBars.Both;
            tbox.Font = new Font("Arial", 12, FontStyle.Bold);
            tbox.BackColor = Color.DarkGray;
            tbox.ForeColor = Color.Black;
            tbox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Form2.Controls.Add(tbox);

            if (linesToDisplay == null) {
                throw new NullReferenceException();
            }

            string theWholeThing = "";
            foreach (string line in linesToDisplay)
            {
                //tbox.AppendText(line);
                //tbox.AppendText(Environment.NewLine);
                theWholeThing += line + Environment.NewLine;
            }
            tbox.AppendText(theWholeThing);
            tbox.Select(0, 0);
            tbox.ScrollToCaret();
        }

        private void UpdateSelectionBox(System.Windows.Forms.TextBox box)
        {
            box.Clear();
            foreach (string f in fileSelection)
            {
                box.AppendText(String.Format("-> {0}", Path.GetFileName(f)));
                box.AppendText(Environment.NewLine);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {

            //  CheckBox2 = titles
            //  CheckBox3 = duplicates

            bool keeptitles = CheckBox2.Checked;
            bool removeduplicates = CheckBox3.Checked;
            bool newlines = CheckBox4.Checked;

            int lineCount = 0;
            var myData = e.Argument as MyDataObject;

            List<string> unsorted = myData.unsortedlines;
            List<string> unsortedSettings = new List<string>();

            List<string> master = myData.masterlines;
            List<string> masterSettings = new List<string>();

            List<string> sorted = new List<string>();
            
            List<string> unused = new List<string>();

            

            isRunning = true;
            
            Console.WriteLine(
                String.Format(
                    "Started bgWorker1 (unsorted: {0}, master: {1}), titles={2}, duplicateremoval={3}, newlines={4}", 
                                unsorted.Count, 
                                master.Count, 
                                keeptitles, 
                                removeduplicates,
                                newlines));

            if (backgroundWorker1.CancellationPending) {
                e.Cancel = true;
                return;
            }
            else {
                // Build unsortedSettings list
                foreach (string str in unsorted) {
                    if (str.IndexOf(';') == 0) {
                        unused.Add(str);
                        continue;
                    }


                    if (String.IsNullOrEmpty(str)) {
                        unsortedSettings.Add(str);  // DO NOT remove newlines at this point
                        continue;
                    }

                    // Handle section titles
                    if (str.IndexOf('[') != -1) {
                        unsortedSettings.Add(str);
                        continue;
                    }

                    string tmp = Formatter(str)[0];
                    if (String.IsNullOrEmpty(tmp)) { continue; }

                    //unsortedSettings.Add(tmp.ToLower());
                    unsortedSettings.Add(tmp);
                }

                // Duplicate removal
                if (removeduplicates) {
                    List<string> temp = new List<string>();

                    //unsortedSettings = unsortedSettings.Distinct().ToList();

                    // Find difference
                    //dumpToFile(temp.Except(unsortedSettings).ToList(), "temp.ini");

                    int i = 0;

                    foreach (string s in unsortedSettings) {
                        if (!temp.Contains(s)) {
                            //Console.WriteLine(String.Format("temp does not contain: {0}. Adding...", s));
                            temp.Add(s);
                            i += 1;
                            continue;
                        }

                        if (unsortedSettings.GetRange((i + 1),(unsortedSettings.Count - i - 1)).Contains(s)) {
                            temp.Add("");
                            unused.Add(unsorted[i]); // Experimental
                            i += 1;
                            continue;
                        }

                        if (!temp.Contains(s)) {
                            temp.Add(s);
                            i += 1;
                            continue;
                        }

                        temp.Add("");
                        unused.Add(unsorted[i]); // Experimental
                        i += 1;
                    }

                    dumpToFile(temp, "temp.ini");
                    //unsortedSettings = temp;
                } 

                // Build masterSettings list
                foreach (string m in master) {
                    if (String.IsNullOrEmpty(m))
                    {
                        //masterSettings.Add(m);
                        continue;
                    }

                    // Handle section titles
                    if (m.IndexOf('[') != -1)
                    {
                        masterSettings.Add(m);
                        continue;
                    }

                    string tmp = Formatter(m)[0];
                    if (String.IsNullOrEmpty(tmp)) { continue; }

                    //masterSettings.Add(tmp.ToLower());
                    masterSettings.Add(tmp);
                }


                // Main sorting part
                foreach (string m in masterSettings)
                {
                    if (backgroundWorker1.CancellationPending){
                        e.Cancel = true;
                        return;
                    }

                    lineCount += 1;
                    

                    // Update progress
                    percentComplete = (int)((float)((lineCount * 100) / master.Count));
                    if (percentComplete > highestPercentageReached)
                    {
                        highestPercentageReached = percentComplete;
                        backgroundWorker1.ReportProgress(percentComplete);
                    }

                    // Skip empty
                    if (string.IsNullOrEmpty(m)){
                            //Console.WriteLine("skip....");
                            continue;
                    }


                    // Handle titles
                    if (m.IndexOf('[') != -1) {
                        if (!CheckBox2.Checked) continue;

                        if (unsortedSettings.Contains(m)) {
                            Console.WriteLine(String.Format("Title: {0}, Adding to sorted...", m));
                            sorted.Add(m);  
                        }
                        continue;
                    }



                    if (unsortedSettings.Contains(m)) {
                        if (removeduplicates) // Duplicates get removed above
                        {
                            Console.WriteLine(String.Format("Found {0}. Adding: {1}", m, unsorted[unsortedSettings.IndexOf(m)]));
                            sorted.Add(unsorted[unsortedSettings.IndexOf(m)]);
                        }
                        else {
                            //Console.WriteLine(String.Format("-> Adding first : {0}", unsorted[unsortedSettings.IndexOf(m)]));
                            //sorted.Add(unsorted[unsortedSettings.IndexOf(m)]);  // Add first found setting
                            
                            List<int> indices = GetAllIndices(unsortedSettings, m, true); // Find any duplicates

                            if (indices.Count < 1) continue;

                            if (indices.Count == 1)
                            {
                                Console.WriteLine(String.Format("Found {0} of {1}. Adding...", indices.Count, m));
                                sorted.Add(unsorted[unsortedSettings.IndexOf(m)]);
                                continue;
                            }

                            Console.WriteLine(String.Format("Found {0} of {1}.", indices.Count ,m));

                            int c = 0;
                            int found = 0;

                            foreach (string s in unsorted) {
                                if (found == indices.Count) break;

                                if (indices.Contains(c)) {
                                    Console.WriteLine(String.Format("c={0} -> Adding: {1}", c ,unsorted[c]));
                                    sorted.Add(unsorted[c]);
                                    found += 1;
                                }
                                else{
                                    Console.WriteLine(String.Format("c={0}.", c));
                                }

                                c += 1;
                            }
                        }                  
                    }
                }

                // Separate sections with newlines
                if (newlines)
                {
                    string prev = null;
                    List<string> tempSorted = new List<string>();

                    foreach (string s in sorted)
                    {
                        if (string.IsNullOrEmpty(prev)){
                            tempSorted.Add(s);
                            prev = s;
                            continue;
                        }

                        // Titles
                        if(s.IndexOf('[') != -1)
                        {
                            tempSorted.Add("");
                            tempSorted.Add(s);
                            prev = s;
                            continue;
                        }

                        tempSorted.Add(s);
                        prev = s;
                    }
                    sorted = tempSorted;
                }
                

                
                //unused = unsorted.Except(sorted).ToList();
                sorted.Add("");
                sorted.Add("");

                // Append unused lines
                foreach (string s in unused) {
                    if ((!CheckBox2.Checked) && (s.IndexOf('[') != -1)) continue;

                    if (String.IsNullOrEmpty(s)) continue;

                    sorted.Add(String.Format(";{0}", s));
                } 
            }

            // DEBUG ONLY
            //dumpToFile(unsorted, "unsorted.ini");
            dumpToFile(unsortedSettings, "unsortedSettings.ini");
            dumpToFile(masterSettings, "masterSettings.ini");

            e.Result = sorted;
            return;
        }

        private List<int> GetAllIndices(List<string> list, string match, bool caseSensitive=true) {
            List<int> result = new List<int>();
            int i = 0;
            if ((match == null) ||(list.Count <= 1)) return result;


            foreach (string s in list) {
                if (!caseSensitive) {
                    if (s.Equals(match.ToLower(),StringComparison.CurrentCultureIgnoreCase))
                    {
                        result.Add(i);
                        i += 1;
                        continue;
                    }
                }

                if (s.Equals(match)) {
                    result.Add(i);
                }

                i += 1;
            }


            return result;
        }


        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {

            ProgressBar1.Value = e.ProgressPercentage;
            //Console.WriteLine(this.ProgressBar1.Value);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                ConsoleBox1.AppendText("Operation cancelled...\n");
            }
            else {
                sortedLines = ((System.Collections.IEnumerable)e.Result)
                              .Cast<object>()
                              .Select(x => x.ToString())
                              .ToArray();

                
                OutputForm(sortedLines);
                Console.WriteLine("-> Outputting to new form...");

                ConsoleBox1.AppendText("Finished sorting.");
                ConsoleBox1.AppendText(Environment.NewLine);
            }


            ProgressBar1.Value = 0;
            ProgressBar1.Hide();

            CancelButton1.Enabled = false;
            SortButton1.Enabled = true;
            //ProgressBar1.Increment(100);
            
            FileDropdown1.Enabled = true;
            SelectionBox1.Enabled = true;
            DropdownBox1.Enabled = true;

            ToggleSORT(true);
        }

       
        // <EVENTS>

        private void Form1_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var f in files)
            {
                var lineCount = File.ReadLines(@f).Count();
                ConsoleBox1.AppendText(String.Format("Added file: {0} ({1} lines)", (string)f, lineCount));
                ConsoleBox1.AppendText(Environment.NewLine);
            }

            string var = DropdownBox1.Text;
            switch (var){

                case "SORT":
                    string tmp = files.First();
                    if (OpenFileButton1.Visible){
                        unsortedFilePath = Path.GetFullPath(tmp);
                        OpenFileButton1.Text = "Change";

                        Label2.Show();
                        Label3.Show();
                        Label4.Text = Path.GetFileName(tmp);
                        Label4.Show();
                        
                        FileDropdown1.Show();
                        CheckBox2.Show();
                        CheckBox2.Checked = true;
                        CheckBox3.Show();
                        CheckBox4.Show();
                        SortButton1.Show();

                        if (File.Exists(unsortedFilePath)) {
                            unsortedFileLines = ReadFile(Path.GetFullPath(unsortedFilePath),false);
                            FileDropdown1.SelectedIndex = 0;
                            FileDropdown1_SelectedIndexChanged(new object(), new EventArgs());

                            Console.WriteLine("unsorted len:" + unsortedFileLines.Length.ToString());
                        } else {
                            unsortedFilePath = null;
                            Label4.Text = "";
                            Label4.Hide();
                            Label2.Hide();
                            FileDropdown1.Hide();
                            OpenFileButton1.Text = "Open...";
                        }
                    }
                    break;

                case "Identical lines":
                    foreach (string f in files) {
                        if (File.Exists(f)) {
                            fileSelection.Add(f);
                        }
                    }
                    UpdateSelectionBox(SelectionBox1);
                    if (fileSelection.Count >= 2) { CompareFilesButton1.Show(); }
                    else { CompareFilesButton1.Hide(); }
                    break;

                case "Common settings":
                    foreach (string f in files)
                    {
                        if (File.Exists(f))
                        {
                            fileSelection.Add(f);
                        }
                    }
                    UpdateSelectionBox(SelectionBox2);
                    if (fileSelection.Count >= 2) { CompareFilesButton2.Show(); }
                    else { CompareFilesButton2.Hide(); }
                    break;

                case "Difference":
                    foreach (string f in files) {
                        if (File.Exists(f)) {
                            fileSelection.Add(f);
                        }
                    }
                    UpdateSelectionBox(SelectionBox3);
                    if (fileSelection.Count >= 2) { CompareFilesButton3.Show(); }
                    else { CompareFilesButton3.Hide(); }
                    break;
                default:
                    break;
                    
            }
        }


        // SORT

        private void DropdownBox1_DropDownClosed(object sender, EventArgs e){
            HandleDropdown();
        }

        private void OpenFileButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.DefaultExt = "ini";
            d.Filter = "Ini files (*.ini)|*.ini|All files (*.*)|*.*";
            d.Multiselect = false;
            d.Title = "Open file for sorting";
            if (d.ShowDialog() == DialogResult.OK)
            {
                unsortedFilePath = d.FileName;
                string ext = Path.GetExtension(unsortedFilePath);
                List<string> allowedExtensions = new List<string>() {".ini",".txt"};
                if (!allowedExtensions.Contains(ext)) {
                    Label4.Text = "";
                    MessageBox.Show("Incompatible file type!");
                    return;
                }
                string filename = d.FileName;;
                Label4.Text = Path.GetFileName(filename);
                Label4.Show();

                Label2.Show();
                FileDropdown1.Show();
                FileDropdown1.SelectedIndex = 0;
                FileDropdown1_SelectedIndexChanged(new object(), new EventArgs());
                CheckBox2.Show();
                CheckBox2.Checked = true;
                CheckBox3.Show();
                CheckBox4.Show();
                SortButton1.Show();
                CancelButton1.Hide();

                unsortedFilePath = Path.GetFullPath(filename);
                OpenFileButton1.Text = "Change";

                // Read from file
                unsortedFileLines = ReadFile(Path.GetFullPath(unsortedFilePath), false);
                ConsoleBox1.AppendText(String.Format("Selected file: {0} ({1} lines)", d.FileName, unsortedFileLines.Length ));
                ConsoleBox1.AppendText(Environment.NewLine);
            }
            else {
                //ConsoleBox1.AppendText("ERROR: failed opening file...\n");
                unsortedFilePath = null;
                Label4.Text = "";
                Label4.Hide();
                Label2.Hide();
                FileDropdown1.Hide();
                OpenFileButton1.Text = "Open...";
            }
        }

        private void FileDropdown1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FileDropdown1.SelectedIndex > -1)
            {
                masterFilePath = files[FileDropdown1.SelectedIndex];
                masterFileLines = ReadFile(Path.GetFullPath(masterFilePath), false);
                Console.WriteLine("master path:" + masterFilePath);
                Console.WriteLine("master len:" + masterFileLines.Length.ToString());
                CheckBox2.Show();
                CheckBox3.Show();
                CheckBox4.Show();
                SortButton1.Show();
                CancelButton1.Hide();
                ProgressBar1.Show();
            }
            else {
                CheckBox2.Hide();
                CheckBox3.Hide();
                CheckBox4.Hide();
                SortButton1.Hide();
                CancelButton1.Hide();
                ProgressBar1.Hide();
            }
        }


        private void SortButton1_Click(object sender, EventArgs e)
        {
            CancelButton1.Enabled = true;
            CancelButton1.Show();
            SortButton1.Enabled = false;
            FileDropdown1.Enabled = false;
            SelectionBox1.Enabled = false;
            DropdownBox1.Enabled = false;

            if (unsortedFileLines.Length == 0)
            {
                ConsoleBox1.AppendText("WARNING: User file is empty");
                ConsoleBox1.AppendText(Environment.NewLine);
                return;
            }

            if (masterFileLines.Length == 0)
            {
                ConsoleBox1.AppendText("WARNING: Master file is empty");
                ConsoleBox1.AppendText(Environment.NewLine);
                return;
            }

            // HandleSort();

            if (!isRunning)
            {
                var myData = new MyDataObject();
                myData.unsortedlines = new List<string>(unsortedFileLines);
                myData.masterlines = new List<string>(masterFileLines);

                backgroundWorker1.RunWorkerAsync(myData);
                ProgressBar1.Visible = true;
                return;
            }
        }

        private void OpenFileButton2_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.DefaultExt = "ini";
            d.Filter = "Ini files (*.ini)|*.ini|All files (*.*)|*.*";
            d.Multiselect = false;
            d.Title = "Add file";
            if (d.ShowDialog() == DialogResult.OK)
            {
                string filepath = d.FileName;
                Console.WriteLine(filepath);
                if (!fileSelection.Contains(filepath)) {
                    fileSelection.Add(filepath);
                }
                
                // Entry example:
                // file1  [X]
                // file2  [X]

                // Handle drag & drop ??

                string tmp = Path.GetFullPath(filepath);
            }

            SelectionBox1.Clear();

            if (fileSelection.Count >= 2) {
                CompareFilesButton1.Show();
            }
            UpdateSelectionBox(SelectionBox1);
        }

        private void ClearSelectionButton1_Click(object sender, EventArgs e)
        {
            fileSelection = new List<string>();
            SelectionBox1.Clear();
            CompareFilesButton1.Hide();
        }

        private void CompareFilesButton1_Click(object sender, EventArgs e)
        {
            identicalLines = FindIdenticalLines();
            OutputForm(identicalLines);
        }

        private void CancelButton1_Click(System.Object sender, System.EventArgs e)
        {
            this.backgroundWorker1.CancelAsync();
            CancelButton1.Enabled = false;
        }



        private void CompareFilesButton2_Click(object sender, EventArgs e)
        {
            //commonSettings = FindCommonSettings();
            commonSettings = FindCommon(fileSelection,true);
            OutputForm(commonSettings);
        }

        private void ClearSelectionButton2_Click(object sender, EventArgs e)
        {
            fileSelection = new List<string>();
            SelectionBox2.Clear();
            CompareFilesButton2.Hide();
        }

        private void OpenFileButton3_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.DefaultExt = "ini";
            d.Filter = "Ini files (*.ini)|*.ini|All files (*.*)|*.*";
            d.Multiselect = false;
            d.Title = "Add file";
            if (d.ShowDialog() == DialogResult.OK)
            {
                string filepath = d.FileName;
                Console.WriteLine(filepath);
                if (!fileSelection.Contains(filepath))
                {
                    fileSelection.Add(filepath);
                }
                string tmp = Path.GetFullPath(filepath);
            }

            SelectionBox2.Clear();

            if (fileSelection.Count >= 2)
            {
                CompareFilesButton2.Show();
            }
            UpdateSelectionBox(SelectionBox2);
        }



        

        private void OpenFileButton4_Click(object sender, EventArgs e){
            OpenFileDialog d = new OpenFileDialog();
            d.DefaultExt = "ini";
            d.Filter = "Ini files (*.ini)|*.ini|All files (*.*)|*.*";
            d.Multiselect = false;
            d.Title = "Add file";
            if (d.ShowDialog() == DialogResult.OK){
                string filepath = d.FileName;
                Console.WriteLine(filepath);
                if (!fileSelection.Contains(filepath))
                {
                    fileSelection.Add(filepath);
                }
                string tmp = Path.GetFullPath(filepath);
            }

            SelectionBox3.Clear();

            if (fileSelection.Count >= 2)
            {
                CompareFilesButton3.Show();
            }
            UpdateSelectionBox(SelectionBox2);
        }

        private void ClearSelectionButton3_Click(object sender, EventArgs e){
            fileSelection = new List<string>();
            SelectionBox3.Clear();
            CompareFilesButton3.Hide();
        }

        private void CompareFilesButton3_Click(object sender, EventArgs e){
            differentSettings = FindDiffSettings();
        }


        // </EVENTS>        
    }

    public class MyDataObject
    {
        public List<string> unsortedlines;
        public List<string> masterlines;

        public MyDataObject()
        {

        }
    }
}


