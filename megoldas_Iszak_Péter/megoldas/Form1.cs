using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace laprendeles
{
    public partial class Form1 : Form
    {
        private SQLiteConnect DB = new SQLiteConnect("kezbesito.db");
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            X.kapcsolodik();
            List<string[]> temak = X.lekerdez("select distinct tema from lap order by tema");
            foreach (var t in temak) checkedListBox1.Items.Add(t[0]);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            X.kapcsolatBont();
        }

        private void kilépésToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            ((DataGridView)sender).ClearSelection();
        }

        private void lapTáblaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            racs.BringToFront();
            racs.DataSource = X.tablaCsinal("select * from lap");
        }

        private void előfizetőTáblaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            racs.BringToFront();
            racs.DataSource = X.tablaCsinal("select * from elofizeto");
        }

        private void előfizetésTáblaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            racs.BringToFront();
            racs.DataSource = X.tablaCsinal("select * from elofizetes");
        }

        private void haviElőfizetésselRendelhetőLapokToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "Azok a lapok, amikre elő lehet fizetni havi előfizetéssel:";
            racs.BringToFront();
            racs.DataSource = X.tablaCsinal(@"
                select cim, havi from lap
                where havi is not null
                order by cim
            ");
        }

        private void akikLegalább4LapraFizettekElőToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "Akik legalább 4 lapra előfizettek:";
            racs.BringToFront();
            racs.DataSource = X.tablaCsinal(@"select nev, utca, hazszam
                from elofizeto
	                inner join elofizetes on elofizeto.id = elofizetes.eloid
                group by nev
                having count(*) >= 4
            ");
        }

        private void bodorUtca13ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            X.parancs.CommandText = @"
                select sum(eves)
                from elofizeto
	                inner join elofizetes on elofizeto.id = elofizetes.eloid
	                inner join lap on lap.id = elofizetes.lapid
                where utca = 'Bodor utca' and hazszam = 13
            ";
            int fizetendo = Convert.ToInt32(X.parancs.ExecuteScalar());
            string s = $"A Bodor utca 13. alatt lakók {fizetendo:c0}-ot " +
                "fizetnének éves előfizetések esetén.";
            X.uzen(s);
        }

        private void aLegnagyobbKedvezményÉvesElőfizetésselToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string sql = @"
                select cim, havi * 12, eves, havi * 12 - eves as k
                from lap
                where havi is not null
                order by k desc limit 1
            ";
            string[] t = X.lekerdez(sql)[0];
            string s = $@"Lap címe: {t[0]}
                - havi előfizetés esetén évi {int.Parse(t[1]):c0},
                - éves előfizetés esetén {int.Parse(t[2]):c0}.

                A kedvezmény {int.Parse(t[3]):c0}.
            ".Replace("  ", "");
            X.uzen(s);
        }

        private void aMagyarNemzetElőfizetőiTöbbiLapjaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "A Magyar Nemzet előfizetői ezekre fizettek még elő:";
            racs.BringToFront();
            racs.DataSource = X.tablaCsinal(@"
                with seged as
                (
	                select eloid
	                from elofizetes inner join lap on elofizetes.lapid = lap.id
	                where cim = 'Magyar Nemzet'
                )
                select distinct cim
                from lap
                    inner join elofizetes on lap.id = elofizetes.lapid
                    inner join seged on seged.eloid = elofizetes.eloid
                where cim != 'Magyar Nemzet'
            ");
        }

        private void akikCsakHetilapraFizettekElőToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "Akik csak hetilapra fizettek elő:";
            racs.BringToFront();
            racs.DataSource = X.tablaCsinal(@"
                with seged as
                (
	                select distinct eloid
	                from lap inner join elofizetes on lap.id = elofizetes.lapid
	                where gyakorisag != 52
                )
                select nev, utca, hazszam, cim
                from lap
	                inner join elofizetes on lap.id = elofizetes.lapid
	                inner join elofizeto on elofizetes.eloid = elofizeto.id
                where elofizeto.id not in (select eloid from seged)
            ");
        }

        private void azÉviMax12szerMegjelenőLapokToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "Azok a lapok, amik max. 12-szer jelennek meg egy évben:";
            textBox1.BringToFront();
            textBox1.Visible = false;
            List<string[]> temp = X.lekerdez(@"
                select tema, gyakorisag, cim
                from lap
                where gyakorisag <= 12
            ");
            //0. téma, 1. gyakoriság, 2. cím
            textBox1.Text = "Témakör    Évi lapszám    Cím" + nl;
            var temakorok = temp.Select(a => a[0]).Distinct().OrderBy(a => a);
            int maxH = temakorok.Max(a => a.Length);
            foreach (string tem in temakorok)
            {
                textBox1.AppendText(string.Format("{0,-" + maxH + "}{1}", tem, nl));
                var gyakorisagok = temp.Where(a => a[0] == tem).Select(a => int.Parse(a[1])).Distinct().OrderBy(a => a);
                foreach (var gya in gyakorisagok)
                {
                    string s = string.Format("{0," + (maxH + 10) + "}{1}", gya, nl);
                    textBox1.AppendText(s);
                    foreach (var t in temp.Where(a => a[0] == tem && int.Parse(a[1]) == gya))
                    {
                        s = string.Format("{2," + (maxH + 18) + "}{0}{1}", t[2], nl, " ");
                        textBox1.AppendText(s);
                    }
                }
            }
            textBox1.Visible = true;
        }

        string nl = Environment.NewLine;

        private void textBox1_Enter(object sender, EventArgs e)
        {
            menuStrip1.Focus();
        }

        private void hogyanNEMFizethetünkElőALapokraIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "Ahogy a lapokra nem lehet előfizetni:";
            racs.BringToFront();
            racs.DataSource = X.tablaCsinal(@"
                select cim,
	                case when havi is null then 'X' else '' end as havi,
	                case when negyedeves is null then 'X' else '' end as negyedeves,
	                case when feleves is null then 'X' else '' end as feleves
                from lap
                where havi is null or negyedeves is null or feleves is null 
                order by cim
            ");
        }

        private void hogyanNEMFizethetünkElőALapokraIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "Ahogy a lapokra nem lehet előfizetni:";
            racs.BringToFront();
            List<string[]> temp = X.lekerdez(@"
                with seged as
                (select cim, havi, negyedeves, feleves from lap order by cim)
                select
	                case when havi is null then cim else '' end as havi,
	                case when negyedeves is null then cim else '' end as negyedeves,
	                case when feleves is null then cim else '' end as feleves
                    from seged
                    where havi is null or negyedeves is null or feleves is null
            ");
            List<string> haviak = temp.Select(a => a[0]).Where(a => a != "").ToList();
            List<string> negyedevesek = temp.Select(a => a[1]).Where(a => a != "").ToList();
            List<string> felevesek = temp.Select(a => a[2]).Where(a => a != "").ToList();
            List<string[]> li = new List<string[]>();
            int n = new[] { haviak, negyedevesek, felevesek }.Max(a => a.Count);
            //n = Math.Max(haviak.Count, Math.Max(negyedevesek.Count, felevesek.Count));
            for ( int i = 0; i < n; i++)
            {
                string[] t = new string[3];
                //if (i < haviak.Count) t[0] = haviak[i]; else t[0] = "";
                t[0] = i < haviak.Count ? haviak[i] : "";
                t[1] = i < negyedevesek.Count ? negyedevesek[i] : "";
                t[2] = i < felevesek.Count ? felevesek[i] : "";
                li.Add(t);
            }
            racs.DataSource = li
                .Select(a => new
                {
                    havi = a[0],
                    negyedéves = a[1],
                    féléves = a[2]
                }).ToList();
        }

        private void adottTémájúLapokToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "Lapok keresése témák szerint";
            panel1.BringToFront();

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var bekapcsoltak = checkedListBox1.CheckedItems;
            List<string> temp = new List<string>();
            foreach (var elem in bekapcsoltak) temp.Add($"'{elem.ToString()}'");
            string s = string.Join(", ", temp);
            s = "where tema in (" + s + ")";
            racs2.DataSource = X.tablaCsinal($"select * from lap {s}");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                checkedListBox1.SetItemCheckState(i, CheckState.Checked);
            racs2.DataSource = X.tablaCsinal($"select * from lap");
        }

        private void számÁraÉvesElőfizetésEseténToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "1 lapszám ára éves előfizetés esetén";
            racs.BringToFront();
            //racs.DataSource = X.tablaCsinal("select cim cím, round(1.0 * eves / gyakorisag, 1) ár from lap order by ár desc");
            List<string[]> li = X.lekerdez("select cim cím, round(1.0 * eves / gyakorisag, 1) ár from lap order by ár desc");
            racs.DataSource = li
                .Select(a => new
                {
                    cím = a[0],
                    ár = $"{double.Parse(a[1]):c1}"
                }).ToList();
            racs.Columns["ár"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void akikAzonosCímenLaknakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "Akik azonos címen laknak";
            textBox1.BringToFront();
            List<string[]> li = X.lekerdez(@"
                select utca || ' ' || hazszam || '.' as cim, group_concat(nev, ',') 
                from elofizeto
                group by cim
                having count(*) > 1
                order by utca, hazszam
            ");
            foreach (string[] t in li)
            {
                textBox1.AppendText(t[0] + nl);
                string s = t[1].Replace(",", nl + '\t');
                textBox1.AppendText('\t' + s + nl);
            }
        }

        private void napilapokSzámaUtcánkéntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1.Text = "A napilapok száma utcánként";
            racs.BringToFront();
            racs.DataSource = X.tablaCsinal(@"
                select utca, count(*) 'napilapok száma'
                from elofizeto
                    inner join elofizetes on elofizetes.eloid = elofizeto.id
                    inner join lap on elofizetes.lapid = lap.id
                where gyakorisag = 307
                group by utca
            ");
        }

        private void holFizettekElőALegtöbbfajtaLapraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] t = X.lekerdez(@"
                with seged as
                (
	                select distinct utca || ' ' || hazszam || '.' lakcim, lapid
	                from elofizeto inner join elofizetes on elofizeto.id = elofizetes.eloid
                )
                select lakcim, count(*) from seged
                group by lakcim
                order by count(*) desc limit 1
            ")[0];
            string s = $"A legtöbb különböző lap-előfizetés: {t[1]} db;\nezen a címen: {t[0]}";
            X.uzen(s);
        }






        
        private void azEgyesUtcákMelyikLapokatNemRendelikToolStripMenuItem_Click(object sender, EventArgs e) {
            textBox1.BringToFront();

            var utcak = DB.Query( "select utca from elofizeto");
            var lapok = DB.Query( "select cim from lap");
            
            var temp = DB
                .QueryList(
                    "select cim, utca from lap left join elofizetes on lap.id=elofizetes.lapid LEFT join elofizeto on elofizetes.eloid=elofizeto.id ")
                .Select(_ => new {cim = _[0], utca = _[1]})
                .GroupBy(_=>_.cim).Select(_=>new
                {
                    cim=_.Key,
                    nem_fizet=utcak.Where(__=>!_.Select(___=>___.utca).Contains(__)).Distinct()
                }).Where(_=>_.nem_fizet.Any()).Select(_=>_.cim+"\r\n\t"+string.Join("\r\n\t",_.nem_fizet));
            textBox1.Text=string.Join("\r\n",temp);

            temp = DB
                .QueryList(
                    "select cim, utca from lap left join elofizetes on lap.id=elofizetes.lapid LEFT join elofizeto on elofizetes.eloid=elofizeto.id ")
                .Select(_ => new {cim = _[0], utca = _[1]})
                .GroupBy(_=>_.utca).Select(_=>new{
                    utca=_.Key,
                    nem_vesz=lapok.Where(__=>!_.Select(___=>___.cim).Contains(__)).Distinct()
                }).Where(_=>_.utca.Any()).Select(_=>_.utca+"\r\n\t"+string.Join("\r\n\t",_.nem_vesz));
            textBox1.Text=string.Join("\r\n",temp);

        }

        private void zabbOttoKöltözikToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.Text = "Zabb Ottó beköltözik és heti lapra fizrt elő";
            panel2.BringToFront();
            listBox1.Items.AddRange(DB.Query("select DISTINCT utca from elofizeto").ToArray());
            
            
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            listBox2.Items.AddRange(DB.Query($"select DISTINCT hazszam from elofizeto where utca like '{((ListBox)sender).SelectedItem}'").ToArray());   
            button2.Enabled = false;
            listBox3.Items.Clear();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e) {
            listBox3.Items.Clear();
            var sql = $@"select DISTINCT cim  from lap where gyakorisag=52 and id not in (select DISTINCT elofizetes.lapid from elofizetes inner join elofizeto on elofizetes.eloid=elofizeto.id  where utca='{listBox1.SelectedItem}' and hazszam={listBox2.SelectedItem} )";
            listBox3.Items.AddRange(DB.Query(sql).ToArray());
            button2.Enabled = false;
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e) => button2.Enabled = true;

        private void button2_Click(object sender, EventArgs e) {
            var utca = listBox1.SelectedItem.ToString();
            var hazszam = listBox2.SelectedItem.ToString();
            var lap = listBox3.SelectedItem.ToString();
            DB.NoQuery($"insert into elofizeto values(null,'Zabb Ottó','{utca}',{hazszam})");
            try {
                var otto =  DB.QueryOne($"select id from elofizeto where nev || utca || hazszam = 'Zabb Ottó{utca}{hazszam}'");
                var lapid = DB.QueryOne($"select id from lap  where cim='{lap}'");
                X.uzen(1 == DB.NoQuery($"insert into elofizetes values({otto},{lapid},null)")
                    ? $"Zabb Ottó beköltözöik ide: {utca} {hazszam}.\r\nElő fizet erre a lapra: {lap}"
                    : "Ottó mégsem költözik");
            }
            catch (Exception exception) {
                MessageBox.Show(exception.ToString());
            }
        }
    }
}
