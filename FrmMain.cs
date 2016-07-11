using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using Sepehr.Properties;
using System.Net;
using System.Security.Permissions;
using System.Threading;
using Sepehr.DAL;

namespace Sepehr
{
    public partial class FrmMain : Form
    {
        //ErrorNumber : 103102
        public enum AcssesLevel { Admin = 0, Parking = 1, Request = 2, Lading = 3 };
        public enum FormsDefualtShow { FrmMain = 0, FrmParkingEnter = 1, FrmParkingExit = 2, FrmLadingShowBar = 3 };

        public AcssesLevel Level;
        public FormsDefualtShow frmDefualtShow;
        BLL.CsBLLInnings Innings = new BLL.CsBLLInnings();
        BLL.CsBLLSettings BLLSettings = new BLL.CsBLLSettings();

        public string Driver_Selectquery = "";
        public Int32 Driver_rowOffset = 0;
        public Int32 Driver_rowLimit = 30;
        public Int32 Driver_rowCount = 0;

        public string Vehicle_Selectquery = "";
        public Int32 Vehicle_rowOffset = 0;
        public Int32 Vehicle_rowLimit = 30;
        public Int32 Vehicle_rowCount = 0;

        public string Parking_Selectquery = "";
        public Int32  Parking_rowOffset = 0;
        public Int32  Parking_rowLimit = 30;
        public Int32  Parking_rowCount = 0;

        public bool doBackUp = false;
        public Int32 Billing_RowOffset = 0;
        public Int32 Billing_RowLimit = 10;
        public Int32 Billing_RowCount = 0;
        public string Billing_Selectquery = "";

        public Int32 Transaction_RowOffset = 0;
        public Int32 Transaction_RowLimit = 10;
        public Int32 Transaction_RowCount = 0;
        public string Transaction_Selectquery = "";

        public Int32 Travel_RowOffset = 0;
        public Int32 Travel_RowLimit = 20;
        public Int32 Travel_RowCount = 0;
        public string Travel_Selectquery = "";

        public Int32 LastElamBars_rowOffset = 0;
        public Int32 ElamBars_RowLimit = 20;
        public Int32 LastElamBars_rowCount = 0;
        public string LastElamBars_Selectquery = "";

        public string InningList_Selectquery = "";
        public Int32 InningList_rowOffset = 0;
        public Int32 InningList_rowLimit = 30;
        public Int32 InningList_rowCount = 0;

        public int CurentInnings = 0;

        public BindingSource bsParkingList = new BindingSource();
        public BindingSource bsLading = new BindingSource();
        BindingSource bsBar = new BindingSource();
        BindingSource bsTeravelTime = new BindingSource();
        BLL.CsBLLParkingGroup ParkingGroup = new BLL.CsBLLParkingGroup();
        public Int64 editId = 0;

        bool IsSoundLoaded = true;
        public FrmMain()
        {
            InitializeComponent();
            Helper.Settings.SettingsObjectsLoad(this.Name);
            Level = (AcssesLevel)int.Parse(Helper.Settings.ReadObject("FrmMain", "LevelAcsses") == "" ? "0" : Helper.Settings.ReadObject("FrmMain", "LevelAcsses"));
            frmDefualtShow = (FormsDefualtShow)int.Parse(Helper.Settings.ReadObject("FrmMain", "FormsCode") == "" ? "0" : Helper.Settings.ReadObject("FrmMain", "FormsCode"));
            switch (Level)
            {
                case AcssesLevel.Admin:
                    break;
                case AcssesLevel.Parking:
                    menuMain.Visible = false;
                    tabMain.TabPages.Remove(tabCompanyList);
                    tabMain.TabPages.Remove(tabReqest);
                    tabMain.TabPages.Remove(tabHavale);
                    break;
                case AcssesLevel.Request:
                    menuMain.Visible = false;
                    tabMain.TabPages.Remove(tabHavale);
                    tabMain.TabPages.Remove(tbParking);
                    tabMain.TabPages.Remove(tabDriver);
                    tabMain.TabPages.Remove(tabVehicle);
                    tabMain.TabPages.Remove(tabInnings);
                    break;
                case AcssesLevel.Lading:
                    menuMain.Visible = false;
                    tabMain.TabPages.Remove(tbParking);
                    tabMain.TabPages.Remove(tabDriver);
                    tabMain.TabPages.Remove(tabVehicle);
                    tabMain.TabPages.Remove(tabInnings);
                    tabMain.TabPages.Remove(tabReqest);
                    break;

                default:
                    break;
            }

        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            try
            {
                Helper.CsGeneral.frmSplash1.Hide();
                this.WindowState = FormWindowState.Maximized;
                Helper.CsGeneral.myCompany = new BLL.CsBLLCompany();
                Helper.CsGeneral.myCompany = BLL.CsBLLCompany.select();
                if (Helper.CsGeneral.myCompany == null)
                {
                    Helper.CsGeneral.myCompany = new BLL.CsBLLCompany();
                }
                string tmp = this.Text;
                tmp = tmp + " نسخه " + Helper.CsGeneral.frmSplash1.lblVersion.Text.Substring(5) + "    کاربر " + Helper.CsGeneral.loginUser.UserName + " (" + (Helper.CsGeneral.loginUser.Name + " " + Helper.CsGeneral.loginUser.LastName).Trim() + ")";
                this.Text = tmp;
                this.lblCompanyName.Text = Helper.CsGeneral.myCompany.Name;


                Helper.CsHelper.restorGrid(this.dGV_Drivers, 0);

                #region tabReqest -5
                if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(5, 0))
                {
                   
                }
                #endregion
                #region tabBaje -7
                if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(7, 0))
                {
                    this.cmbCapacity = Helper.CsComboBox.setCapacity(cmbCapacity, true, true);
                    this.cmbVechleType = Helper.CsComboBox.setVehicleType(cmbVechleType, true, true);
                }
                #endregion
                btnSetDriverFilter_Click(this,null);
                btnSetVehicleFilter_Click(this, null);
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("19009>" + ex.Message.ToString());
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {

            try
            {
                dgParking.AutoGenerateColumns = false;
                DAL.CsDALAcssesLevel.AcssesUserLoginLoad();
                try
                {
                    // TODO: This line of code loads data into the 'sSDB_Vehicle.Tbl_Vehicle' table. You can move, or remove it, as needed.
                    this.tbl_VehicleTableAdapter.Connection.ConnectionString = Helper.CsGeneral.conStrSepehrDB;
                    this.tbl_VehicleTableAdapter.Fill(this.sSDB_Vehicle.Tbl_Vehicle);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در بارگذاری لیست رانندگان خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                try
                {
                    // TODO: This line of code loads data into the 'sSDB_Drivers.Tbl_Drivers' table. You can move, or remove it, as needed.
                    this.tbl_DriversTableAdapter.Connection.ConnectionString = Helper.CsGeneral.conStrSepehrDB;
                    this.tbl_DriversTableAdapter.Fill(this.sSDB_Drivers.Tbl_Drivers);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در بارگذاری لیست رانندگان خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                try
                {
                    // TODO: This line of code loads data into the 'sPBDB_TransportCompany.Tbl_TransportCompany' table. You can move, or remove it, as needed.
                    this.tbl_TransportCompanyTableAdapter.Connection.ConnectionString = Helper.CsGeneral.conStrSepehrDB;
                    //Helper.CsHelper.showMessage("مرحله 4 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tbl_TransportCompanyTableAdapter.Fill(this.sPBDB_TransportCompany.Tbl_TransportCompany);
                    //Helper.CsHelper.showMessage("مرحله 5 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در بارگذاری لیست شرکت های حمل و نقل خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                try
                {
                    // TODO: This line of code loads data into the 'sPBDB_InningsList.Tbl_InningsList' table. You can move, or remove it, as needed.
                    this.tbl_InningsListTableAdapter.Connection.ConnectionString = Helper.CsGeneral.conStrSepehrDB;
                    //Helper.CsHelper.showMessage("مرحله 6 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tbl_InningsListTableAdapter.Fill(this.sPBDB_InningsList.Tbl_InningsList);
                    //Helper.CsHelper.showMessage("مرحله 7 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در بارگذاری لیست نوبت خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                try
                {
                    // TODO: This line of code loads data into the 'sPBDB_ParkingList.Tbl_ParkingList' table. You can move, or remove it, as needed.
                    this.tbl_ParkingListTableAdapter.Connection.ConnectionString = Helper.CsGeneral.conStrSepehrDB;
                    //Helper.CsHelper.showMessage("مرحله 8 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.tbl_ParkingListTableAdapter.Fill(this.sPBDB_ParkingList.Tbl_ParkingList);
                    //Helper.CsHelper.showMessage("مرحله 9 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: this.tbl_ParkingListTableAdapter.Fill()>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در بارگذاری لیست پارکینگ خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                cmbStatuesParking.SelectedIndex = 0;
                //Helper.CsHelper.showMessage("مرحله 9 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                #region Parking
                try
                {
                    cmbParkingGroup.DataSource = DAL.CsDALParking.ParkingGroupList(true);
                    //Helper.CsHelper.showMessage("مرحله 10 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbParkingGroup.DisplayMember = "Name";
                    cmbParkingGroup.ValueMember = "ID";
                    cmbParkingGroup.SelectedIndex = 0;
                    //Helper.CsHelper.showMessage("مرحله 11 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    this.cmbUserEnter = Helper.CsComboBox.setUserList(this.cmbUserEnter, true);
                    this.CmbUserExit = Helper.CsComboBox.setUserList(this.CmbUserExit, true);

                    //Helper.CsHelper.showMessage("مرحله 12 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: #region Parking>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در ثبت مقادیر اولیه خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                

                #endregion

                #region Inningsنوبت
                try
                {
                    this.cmbUserInningSave = Helper.CsComboBox.setUserList(this.cmbUserInningSave, true);
                    
                    cmbInningsGroup.DisplayMember = "Name";
                    cmbInningsGroup.ValueMember = "ID";
                    cmbInningsGroup.DataSource = DAL.CsDALInningsGroup.SelectAll("",true);
                    //Helper.CsHelper.showMessage("مرحله 13 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    cmbInningsStatue.DisplayMember = "Name";
                    cmbInningsStatue.ValueMember = "ID";
                    cmbInningsStatue.DataSource = DAL.CsDALInningsStatue.selectIsAll("");
                    //Helper.CsHelper.showMessage("مرحله 14 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: #region Innings>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در ثبت مقادیر اولیه خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                #endregion

                #region Lading درخواست بار
                try
                {
                    this.cmbFromLocation = Helper.CsComboBox.setCityList(cmbFromLocation);
                    this.cmbToLocation = Helper.CsComboBox.setCity(cmbToLocation);
                    this.cmbCapacity = Helper.CsComboBox.setCapacity(cmbCapacity, true, true);
                    this.cmbVechleType = Helper.CsComboBox.setVehicleType(cmbVechleType, true, true);
                    //Helper.CsHelper.showMessage("مرحله 15 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: #region Lading>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در ثبت مقادیر اولیه خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                #endregion

                #region Lading باجه
                try
                {
                    this.cmbLFromLocation = Helper.CsComboBox.setCityList(cmbLFromLocation);
                    this.cmbLToLocation = Helper.CsComboBox.setCity(cmbLToLocation);
                    this.cmbLCapacity = Helper.CsComboBox.setCapacity(cmbLCapacity, true, true);
                    this.cmbLVechleType = Helper.CsComboBox.setVehicleType(cmbLVechleType, true, true);
                    //Helper.CsHelper.showMessage("مرحله 16 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: #region Lading>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در ثبت مقادیر اولیه خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                #endregion

                #region Request
                try
                {
                    cmbGroupSelect.DataSource = DAL.CsDALInningsGroup.SelectAll("",true);
                    cmbGroupSelect.DisplayMember = "Name";
                    cmbGroupSelect.ValueMember = "ID";
                    //Helper.CsHelper.showMessage("مرحله 17 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: #region Request>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در ثبت مقادیر اولیه خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                #endregion

                #region TabSettings

                try
                {
                    this.cmbFromTravel = Helper.CsComboBox.setCityList(cmbFromTravel);
                    this.cmbToTravel = Helper.CsComboBox.setCity(cmbToTravel);
                    //Helper.CsHelper.showMessage("مرحله 18 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: #region TabSettings>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در ثبت مقادیر اولیه خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                #endregion
                #region Settings
                try
                {
                    cmbGroupInnings.DataSource = DAL.CsDALInningsGroup.SelectAll("");
                    cmbGroupInnings.DisplayMember = "Name";
                    cmbGroupInnings.ValueMember = "ID";

                    //Helper.CsHelper.showMessage("مرحله 19 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    BLLSettings.SetSettings();
                    if (BLLSettings.id > 0)
                    {
                        txtAddressCameraEnter.Text = BLLSettings.AddressCameraEnter;
                        txtAddressCameraExit.Text = BLLSettings.AddressCameraExit;
                        txtAddressCameraEnter2.Text = BLLSettings.AddressCameraEnter2;
                        txtAddressCameraExit2.Text = BLLSettings.AddressCameraExit2;
                        rdbEnterAndExit1.Checked = Helper.Settings.ReadObject(this.Name, "ParkingEnterAndExit") == "1";
                        rdbEnterAndExit2.Checked = Helper.Settings.ReadObject(this.Name, "ParkingEnterAndExit") == "2";

                        numCountCallForShowAbsence.Value = BLLSettings.CountCallForShowAbsence;
                        txtFerem.Value = BLLSettings.CameraFerem;
                        chbIsIntrim.Checked = BLLSettings.IsInterim;
                        switch (BLLSettings.InningsSaveType)
                        {
                            case Sepehr.BLL.CsBLLSettings.InningsType.IsNotEnterSave:
                                rdbIsNotEnterSave.Checked = true;
                                break;
                            case Sepehr.BLL.CsBLLSettings.InningsType.IsEnterSave:
                                //rdbIsEnterSave.Checked = true;
                                break;
                            case Sepehr.BLL.CsBLLSettings.InningsType.IsParkingSave:
                                rdbIsParkingSave.Checked = true;
                                break;
                            default:
                                break;
                        }
                        chbCancelInnings.Checked = BLLSettings.CancelInnings;
                    }
                    //Helper.CsHelper.showMessage("مرحله 20 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: #region Settings>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در ثبت مقادیر اولیه خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                #region Ladinngs
                try
                {
                    cmbGroupInnings.SelectedValue = Helper.Settings.ReadObject(this.Name, "GroupInnings") == "" ? "0" : Helper.Settings.ReadObject(this.Name, "GroupInnings");
                    txtLedInnings.Text = Helper.Settings.ReadObject(this.Name, "AddressLedInnings");
                    txtBaje.Value = decimal.Parse(Helper.Settings.ReadObject(this.Name, "NumberBaje") == "" ? "0" : Helper.Settings.ReadObject(this.Name, "NumberBaje"));
                    checkPlaySound.Checked = Helper.Settings.ReadObject(this.Name, "IsPlaySound") == "1";
                    //Helper.CsHelper.showMessage("مرحله 21 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: #region Ladinngs>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در ثبت مقادیر اولیه خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                #endregion
                #endregion

                #region Havale
                try
                {
                    comboHavaleGroup.DisplayMember = "Name";
                    comboHavaleGroup.ValueMember = "ID";
                    comboHavaleGroup.DataSource = DAL.CsDALInningsGroup.SelectAll("",true);
                    //Helper.CsHelper.showMessage("مرحله 22 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: #region Havale>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در ثبت مقادیر اولیه خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
               
                #endregion

                try
                {
                    switch (frmDefualtShow)
                    {
                        case FormsDefualtShow.FrmMain:
                            break;
                        case FormsDefualtShow.FrmParkingEnter:
                            new Forms.FrmParkingEnter().ShowDialog();
                            break;
                        case FormsDefualtShow.FrmParkingExit:
                            new Forms.FrmParkingExit().ShowDialog();
                            break;
                        case FormsDefualtShow.FrmLadingShowBar:
                            new Forms.FrmLadingShowBar().ShowDialog();
                            break;
                        default:
                            break;
                    }
                    //Helper.CsHelper.showMessage("مرحله 23 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: switch (frmDefualtShow)>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در نمایش صفحه اصلی خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }


                #region AcssesLevelUser
                SetAcssesLevelUser();
                #endregion


                #region SoundPlay
                try
                {
                    if (Helper.Settings.ReadObject(this.Name, "IsPlaySound") == "1")
                    {
                        try
                        {
                            DAL.CsDALSound.Delete();

                            GetNumbers();
                            IsSoundLoaded = false;
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message);
                        }
                    }
                    //Helper.CsHelper.showMessage("مرحله 24 انجام شد", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Helper.CsHelper.writeLog("FrmMain_Load :: #region SoundPlay>" + ex.Message.ToString());
                    Helper.CsHelper.showMessage("در فعال پاک سازی اطلاعات صدا خطا به وجود آمد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
               
                #endregion


            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("19010>" + ex.Message.ToString());
            }
        }

        private void SetAcssesLevelUser()
        {
            #region tabAlmasInnings -11
            if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(11, 0))
            {
              btnAlmasInsert.Enabled=  DAL.CsDALAcssesLevel.CheackUserLoginAcsses(11, 1);
                  
            }
            else
            {
                tabMain.TabPages.Remove(tabAlmasInnings);
            }
            #endregion
            #region tabSettings -9
            if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(9, 0))
            {
                if (!DAL.CsDALAcssesLevel.CheackUserLoginAcsses(9, 1))
                    tabSettingsSub.TabPages.Remove(tabSettingParking);
                if (!DAL.CsDALAcssesLevel.CheackUserLoginAcsses(9, 2))
                    tabSettingsSub.TabPages.Remove(tabSettingsBaje);
                if (!DAL.CsDALAcssesLevel.CheackUserLoginAcsses(9, 3))
                    tabSettingsSub.TabPages.Remove(tabTeravelTime);

                if (!DAL.CsDALAcssesLevel.CheackUserLoginAcsses(9, 4))
                    tabSettingsSub.TabPages.Remove(tabPageUser);

                if (!DAL.CsDALAcssesLevel.CheackUserLoginAcsses(9, 5))
                    tabSettingsSub.TabPages.Remove(tabDefanction);

                if (!DAL.CsDALAcssesLevel.CheackUserLoginAcsses(9, 6))
                    tabSettingsSub.TabPages.Remove(tabSettingsSystem);
            }
            else
            {
                tabMain.TabPages.Remove(tabSettings);
            }
            #endregion
            #region tabWrongdoer -10
            if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(10, 0))
            {
                btnInsertWrongdoer.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(10, 1);
                btnEditWrongdoer.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(10, 2);
                btnRelease.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(10, 3);
            }
            else
            {
                tabMain.TabPages.Remove(tabWrongdoer);
            }
            #endregion
            #region tabHavale -8
            if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(8, 0))
            {
                btnPrintHavale.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(8, 1);
                btnDeleteHavale.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(8, 2);
                //  btnParkingEdit.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(8, 3);
            }
            else
            {
                tabMain.TabPages.Remove(tabHavale);
            }
            #endregion
            #region tabBaje -7
            if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(7, 0))
            {
                btnJoin.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(7, 1);
                btnShowBar.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(7, 2);

            }
            else
            {
                tabMain.TabPages.Remove(tabBaje);
            }
            #endregion
            #region tabInnings -6
            if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(6, 0))
            {
                btnEnterInnigs.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(6, 1);
                btnInningsChangeStatues.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(6, 2);
                btnChangeInningsStatuesAll.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(6, 3);
                btnReportStatuLog.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(6, 10);
            }
            else
            {
                tabMain.TabPages.Remove(tabInnings);
            }
            #endregion
            #region tabReqest -5
            if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(5, 0))
            {
                btnNewReq.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(5, 1);
                btnEditReq.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(5, 2);
                btnDeleteReq.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(5, 3);
                btnNotActiveRequest.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(5, 4);
                btnCopyBar.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(5, 5);

                this.cmbCompanyList = Helper.CsComboBox.setTransportCompany(cmbCompanyList, true, true);
                this.cmbFromLocation = Helper.CsComboBox.setCityList(cmbFromLocation);
                this.cmbToLocation = Helper.CsComboBox.setCity(cmbToLocation);
            }
            else
            {
                tabMain.TabPages.Remove(tabReqest);
            }
            #endregion
            #region tbParking -4
            if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(4, 0))
            {
                btnParkingEnter.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(4, 1);
                btnParkingExit.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(4, 2);
                btnParkingEdit.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(4, 3);
                btnCancelParking.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(4, 4);
            }
            else
            {
                tabMain.TabPages.Remove(tbParking);
            }
            #endregion
            #region tabVehicle-3
            if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(3, 0))
            {
                BtnAddVehcile.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(3, 1);
                BtnEditVehicle.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(3, 2);
                btnDeleteVehicle.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(3, 3);
                cmbCompanyTransport = Helper.CsComboBox.setTransportCompany(this.cmbCompanyTransport, true, false);
            }
            else
            {
                tabMain.TabPages.Remove(tabVehicle);
            }
            #endregion
            #region tabDriver-2
            if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(2, 0))
            {
                BtnDriverAdd.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(2, 1);
                btnDriverEdit.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(2, 2);
                BtnDriverDelete.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(2, 3);
            }
            else
            {

                tabMain.TabPages.Remove(tabDriver);
            }
            #endregion
            #region tabCompanyList-1
            if (DAL.CsDALAcssesLevel.CheackUserLoginAcsses(1, 0))
            {
                btnTransportCompanyListAdd.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(1, 1);
                btnTransportCompanyListEdit.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(1, 2);
                btnTransportCompanyListDel.Enabled = DAL.CsDALAcssesLevel.CheackUserLoginAcsses(1, 3);
            }
            else
            {
                
                tabMain.TabPages.Remove(tabCompanyList);
            } 
            #endregion

        }

        private void FrmMain_Activated(object sender, EventArgs e)
        {
            this.lblNow.Text = Helper.CsHelper.getPersianDayOfWeek() + " " + Helper.CsHelper.getPersianDate();
        }
        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(!ShowNumberIsFirst)
                trdShowNumber.Abort();
            if(!SoundPlayIsFirst)
                trdSoundPlay.Abort();

            if (doBackUp)
            {
                SqlCommand scom1 = new SqlCommand();
                SqlConnection.ClearAllPools();
                scom1.Connection = new SqlConnection(Helper.CsGeneral.conStrSepehrDB);
                if (scom1.Connection.State == ConnectionState.Closed)
                    scom1.Connection.Open();
                scom1.Connection.Close();
                string dbname = scom1.Connection.Database;
                string dir = "d:\\SepehrBackup";
                string fileName = dir + "\\SepehrBackUp" + Helper.CsHelper.function_DeletecharFromDate(Helper.CsHelper.getPersianDate()) + Helper.CsHelper.getHour().Replace(':', '_') + ".Bak";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                Helper.CsDBBackup.backUp(dbname, fileName, "NOINIT");
            }

        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    doBackUp = false;
                    FrmExit frmExit1 = new FrmExit();
                    frmExit1.chBackUp.Checked = true;
                    if (frmExit1.ShowDialog() != System.Windows.Forms.DialogResult.Yes)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        if (frmExit1.chBackUp.Checked)
                        {
                            doBackUp = true;
                        }
                    }
                }
            }
            catch
            {
            }

            try
            {
            }

            finally
            {
                SqlDependency.Stop(Helper.CsGeneral.conStrSound);
            }
        }

     




      
        private void TSM_Users_Click(object sender, EventArgs e)
        {
            FrmUserList frmUserList1 = new FrmUserList();
            if (frmUserList1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

            }
        }

        private void TSM_Company_Click(object sender, EventArgs e)
        {
            FrmCompanyInfo frmCompanyInfo1 = new FrmCompanyInfo();
            frmCompanyInfo1.setForm();
            frmCompanyInfo1.ShowDialog();
        }


        private void TSM_InningsGroup_Click(object sender, EventArgs e)
        {
            //FrmInningsGroupList frmInningsGroupList1 = new FrmInningsGroupList();
            //frmInningsGroupList1.ShowDialog();
        }

        private void TSM_ReportInnings_Click(object sender, EventArgs e)
        {
            FrmfilterReportInnings frmfilterReportInnings1 = new FrmfilterReportInnings();
            if (frmfilterReportInnings1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string tmpCondition = frmfilterReportInnings1.geInningsListFilter();
                if (tmpCondition.Trim() != "")
                    tmpCondition = " where " + tmpCondition;
                //  BLL.CsBLLInningsList.printReport(tmpCondition,true);
            }
        }


        private void txtSEInningsD_Name_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }

        private void txtSEInningsN_System_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }

        private void txtSEInningsN_VehicleType_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }

        private void txtInningsStatus_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }

        private void txtSEInningsN_Plaque_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }



        private void button17_Click(object sender, EventArgs e)
        {
            this.panel11.Visible = !this.panel11.Visible;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog1 = new FontDialog();
            fontDialog1.Font = this.dGV_TransportCompanyList.RowsDefaultCellStyle.Font;
            if (fontDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                this.dGV_TransportCompanyList.RowsDefaultCellStyle.Font = fontDialog1.Font;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Helper.CsHelper.editGrid(this.dGV_TransportCompanyList);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Helper.CsHelper.saveGrid(this.dGV_TransportCompanyList, 0);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            Helper.CsHelper.restorGrid(this.dGV_TransportCompanyList, 0);
        }

        private void btnTransportCompanyListAdd_Click(object sender, EventArgs e)
        {
            FrmTransportCompany frmTransportCompany1 = new FrmTransportCompany();
            frmTransportCompany1.formMode = Helper.CsType.FormMode.ADD;
            if (frmTransportCompany1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.tbl_TransportCompanyTableAdapter.Fill(this.sPBDB_TransportCompany.Tbl_TransportCompany);
            }
        }

        private void dGV_TransportCompanyList_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = e.RowIndex; i < e.RowCount + e.RowIndex; i++)
            {
                this.dGV_TransportCompanyList.Rows[i].Cells["TC_Row"].Value = (1 + i).ToString();
            }
        }

        private void btnTransportCompanyListEdit_Click(object sender, EventArgs e)
        {
            if (this.dGV_TransportCompanyList.SelectedRows.Count <= 0)
            {
                return;
            }
            DataGridViewRow dgvr = new DataGridViewRow();
            dgvr = this.dGV_TransportCompanyList.SelectedRows[0];
            Int64 idEdit = Int64.Parse(dgvr.Cells["TC_Id"].Value.ToString());
            FrmTransportCompany frmTransportCompany1 = new FrmTransportCompany();
            frmTransportCompany1.formMode = Helper.CsType.FormMode.EDIT;
            frmTransportCompany1.setForm(idEdit);
            if (frmTransportCompany1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.tbl_TransportCompanyTableAdapter.Fill(this.sPBDB_TransportCompany.Tbl_TransportCompany);
                int varIndex = this.tblTransportCompanyBindingSource.Find("Id", idEdit);
                if ((varIndex <= this.dGV_TransportCompanyList.Rows.Count) && (varIndex >= 0))
                    this.dGV_TransportCompanyList.Rows[varIndex].Selected = true;
            }
        }

        private void btnTransportCompanyListDel_Click(object sender, EventArgs e)
        {
            if (this.dGV_TransportCompanyList.SelectedRows.Count <= 0)
            {
                return;
            }
            DataGridViewRow dgvr = new DataGridViewRow();
            dgvr = this.dGV_TransportCompanyList.SelectedRows[0];
            Int64 idEdit = Int64.Parse(dgvr.Cells["TC_Id"].Value.ToString());
            if (Helper.CsHelper.showMessage("آیا شرکت انتخاب شده حذف شود؟؟", "تایید حذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
            {
                BLL.CsBLLTransportCompany.del(idEdit);
                this.tbl_TransportCompanyTableAdapter.Fill(this.sPBDB_TransportCompany.Tbl_TransportCompany);
            }
        }

        private void btnTransportCompanyListShowAll_Click(object sender, EventArgs e)
        {
            this.tbl_TransportCompanyTableAdapter.Fill(this.sPBDB_TransportCompany.Tbl_TransportCompany);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            FrmDriver frmDriver1 = new FrmDriver();
            frmDriver1.ShowDialog();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            frmVehicle frmVehicle1 = new frmVehicle();
            frmVehicle1.ShowDialog();
        }

        private void button206_Click(object sender, EventArgs e)
        {

            FrmDriver frmDriver1 = new FrmDriver();
            frmDriver1.formMode = Helper.CsType.FormMode.ADD;
            if (frmDriver1.showShoddowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FillDriver();


            }


        }

        private void button205_Click(object sender, EventArgs e)
        {
            try
            {
                if (Helper.CsGeneral.MainForm1.dGV_Drivers.Rows.Count == 0)
                    return;
                Int32 indexDisplay = Helper.CsGeneral.MainForm1.dGV_Drivers.FirstDisplayedScrollingRowIndex;
                int Index = Helper.CsGeneral.MainForm1.dGV_Drivers.SelectedRows[0].Index;

                DataGridViewRow dr = Helper.CsGeneral.MainForm1.dGV_Drivers.SelectedRows[0];
                string tmp = dr.Cells["Inttcard"].Value.ToString();
                FrmDriver frmDriver1 = new FrmDriver();
                frmDriver1.formMode = Helper.CsType.FormMode.ADD;
                frmDriver1.doWebQuery = false;
                frmDriver1.setForm(tmp, 0, false);
                if (frmDriver1.showShoddowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.tbl_DriversTableAdapter.Fill(this.sSDB_Drivers.Tbl_Drivers);
                }

                if (indexDisplay >= 0)
                    Helper.CsGeneral.MainForm1.dGV_Drivers.FirstDisplayedScrollingRowIndex = indexDisplay;
                if ((Index <= Helper.CsGeneral.MainForm1.dGV_Drivers.Rows.Count) && (Index >= 0))
                    Helper.CsGeneral.MainForm1.dGV_Drivers.Rows[Index].Selected = true;
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("12021 " + ex.Message.ToString());
            }

        }

        private void button204_Click(object sender, EventArgs e)
        {
            try
            {
                if (Helper.CsGeneral.MainForm1.dGV_Drivers.Rows.Count == 0)
                    return;
                DataGridViewRow dr = Helper.CsGeneral.MainForm1.dGV_Drivers.SelectedRows[0];
                string tmp = dr.Cells["Inttcard"].Value.ToString();
                if (Helper.CsHelper.showMessage("ایا راننده ردیف   " + dr.Cells["Inttcard"].Value.ToString() + " حذف شود ", "تایید حذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                {
                    string var_querystr = "Delete from  Tbl_Drivers where Inttcard='" + tmp + "'"; ;
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = var_querystr;
                    cmd.Connection = Helper.CsGeneral.MainForm1.tbl_DriversTableAdapter.Connection;
                    if (cmd.Connection.State == ConnectionState.Closed)
                        cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    // Helper.CsGeneral.MainForm1.tbl_DriversTableAdapter.Fill(Helper.CsGeneral.MainForm1.sTCDB_Drivers.Tbl_Drivers);
                }
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("43010 " + ex.Message.ToString());
                Helper.CsHelper.showMessage("در انجام عملیات خطا بوجود امد ، لطفا مجددا سعی کنید", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }

        }
        private void button215_Click(object sender, EventArgs e)
        {
            frmVehicle frmVehicle1 = new frmVehicle();
            frmVehicle1.formMode = Helper.CsType.FormMode.ADD;
            if (frmVehicle1.showShoddowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FillVechile();
            }

        }

        private void button214_Click(object sender, EventArgs e)
        {
            if (Helper.CsGeneral.MainForm1.dGV_Vehicle.Rows.Count == 0)
                return;
            try
            {
                DataGridViewRow dr = Helper.CsGeneral.MainForm1.dGV_Vehicle.SelectedRows[0];
                frmVehicle frmVehicle1 = new frmVehicle();
                Int32 index = Helper.CsGeneral.MainForm1.dGV_Vehicle.FirstDisplayedScrollingRowIndex;

                //                        ClassEdit.EditVehicle();
                frmVehicle1.formMode = Helper.CsType.FormMode.EDIT;
                frmVehicle1.formTitle = "ویرایش ناوگان";
                frmVehicle1.doWebQuery = false;
                frmVehicle1.setForm("", int.Parse(dr.Cells["IdVechile"].Value.ToString()), false);
                if (frmVehicle1.showShoddowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FillVechile();
                }
                if (index >= 0)
                    Helper.CsGeneral.MainForm1.dGV_Vehicle.FirstDisplayedScrollingRowIndex = index;

            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("12003 " + ex.Message.ToString());
                return;
            }
            ///////

        }

        private void button213_Click(object sender, EventArgs e)
        {
            try
            {
                if (Helper.CsGeneral.MainForm1.dGV_Vehicle.Rows.Count == 0)
                    return;
                DataGridViewRow dr = Helper.CsGeneral.MainForm1.dGV_Vehicle.SelectedRows[0];
                Int64 tmpEditId = Int64.Parse(dr.Cells["IdVechile"].Value.ToString());
                if (Helper.CsHelper.showMessage(" ایا ناوگان ردیف " + dr.Cells["IdVechile"].Value.ToString() + " حذف شود ", "تایید حذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                {
                    string var_querystr = "Delete from  Tbl_Vehicle where Id='" + tmpEditId + "'"; ;
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = var_querystr;
                    cmd.Connection = new SqlConnection(Helper.CsGeneral.conStrSepehrDB);
                    if (cmd.Connection.State == ConnectionState.Closed)
                        cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    FillVechile();
                }
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("43011 " + ex.Message.ToString());
                Helper.CsHelper.showMessage("در انجام عملیات خطا بوجود امد ، لطفا مجددا سعی کنید", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            new Forms.FrmParkingExit().ShowDialog();
            FillParking();
        }

        private void tblParkingListBindingSource_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void tbParking_Enter(object sender, EventArgs e)
        {

            //btnSetParkingFilter_Click(this, null);
            //gridSearchParking.Filter();
        }

        private void FillParking()
        {
            try
            {
                //dgParking.AutoGenerateColumns = false;
                //this.txtVehicleTypes = Helper.CsComboBox.setVehicleTypesList(this.txtVehicleTypes, false);
                //rangeParking.CountNumber = DAL.CsDALParking.GetAllParkingCount(cmbStatuesParking.SelectedIndex);
                //bsParkingList.DataSource = DAL.CsDALParking.GetAllParking(cmbStatuesParking.SelectedIndex,rangeParking.Min,rangeParking.Max);
                //dgParking.DataSource = bsParkingList.DataSource;
                //setRowNumber(dgParking);
                btnSetParkingFilter_Click(this, null);

            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FillParking()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("در انجام عملیات خطا بوجود امد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
          
        }


        private void cmbStatuesParking_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (cmbStatuesParking.SelectedIndex > -1)
            //{
            //    dgParking.AutoGenerateColumns = false;
            //    rangeParking.CountNumber = DAL.CsDALParking.GetAllParkingCount(cmbStatuesParking.SelectedIndex);
            //    bsParkingList.DataSource = DAL.CsDALParking.GetAllParking(cmbStatuesParking.SelectedIndex,rangeParking.Min,rangeParking.Max);
            //    dgParking.DataSource = bsParkingList;
             
            //}
            btnSetParkingFilter_Click(sender, e);

        }

        BindingSource BsDriver = new BindingSource();
        private void FillDriver()
        {
            //dGV_Drivers.AutoGenerateColumns = false;
            //BsDriver.DataSource = DAL.CsDALDriver.SelectAll("");
            //dGV_Drivers.DataSource = BsDriver;
            this.tbl_DriversTableAdapter.Fill(this.sSDB_Drivers.Tbl_Drivers);
           
        }
        BindingSource BsVechile = new BindingSource();
        private void FillVechile()
        {
            dGV_Vehicle.AutoGenerateColumns = false;
            BsVechile.DataSource = DAL.CsDALVehicle.SelectAll("");
            dGV_Vehicle.DataSource = BsVechile;
           
        }

        private void button27_Click(object sender, EventArgs e)
        {
            //if (dgParking.SelectedRows.Count > 0)
            //{
            //    Forms.FrmParking2 FrmParking = new Forms.FrmParking2(int.Parse(dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["ID"].Value.ToString()), int.Parse(dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["IDEnterAndExit"].Value.ToString()));
            //    FrmParking.ShowDialog();
            //    FillParking();
            //}

        }
        #region Request
        BindingSource bsRequest = new BindingSource();
        private void FillRequest()
        {
            GridRequest.AutoGenerateColumns = false;
            bsRequest.DataSource = DAL.CsDALRequestVehicle.Select("");
            GridRequest.DataSource = bsRequest;
            //setRowNumber(GridRequest);
        }
        private void FilterRequest()
        {
            string Filter = "1=1 ";


            if (cmbCompanyList.SelectedItem != null && cmbCompanyList.SelectedItem != DBNull.Value && cmbCompanyList.SelectedItem.ToString() != "")
            {
                Filter += " AND CONVERT( NameCompany, 'System.String') ='" + cmbCompanyList.SelectedItem.ToString() + "'";
            }
            if (cmbGroupSelect.SelectedValue.ToString() != "" && cmbGroupSelect.SelectedValue.ToString() != "0")
            {
                Filter += " and CONVERT( InningsGroupID, 'System.String')  =" + cmbGroupSelect.SelectedValue.ToString();
            }
            if (cmbFromLocation.SelectedItem != null && cmbFromLocation.SelectedItem != DBNull.Value && cmbFromLocation.SelectedItem.ToString() != "")
            {
                Filter += " AND CONVERT( CityFrom, 'System.String') ='" + cmbFromLocation.SelectedItem.ToString() + "'";
            }
            if (cmbToLocation.SelectedItem != null && cmbToLocation.SelectedItem != DBNull.Value && cmbToLocation.SelectedItem.ToString() != "")
            {
                Filter += " AND CONVERT( CityTO, 'System.String') ='" + cmbToLocation.SelectedItem.ToString() + "'";
            }
            if (cmbVechleType.SelectedItem != null && cmbVechleType.SelectedItem != DBNull.Value && cmbVechleType.SelectedItem.ToString() != "")
            {
                Filter += " AND CONVERT( NameVehicleType, 'System.String') ='" + cmbVechleType.SelectedItem.ToString() + "'";
            }
            if (cmbCapacity.SelectedItem != null && cmbCapacity.SelectedItem != DBNull.Value && cmbCapacity.SelectedItem.ToString() != "")
            {
                Filter += " AND CONVERT( NameCapacity, 'System.String') ='" + cmbCapacity.SelectedItem.ToString() + "'";
            }
            if (txtCodeElameBar.Text!="")
            {
                Filter += " AND CONVERT( ID, 'System.String') Like'" + txtCodeElameBar.Text+ "%'";
            }
            DateTime dt = new DateTime();
            if (Helper.ConvertDate.PerToGre(txtDateRequest.Text) != dt)
            {
                Filter += " And CONVERT( DateShamsi, 'System.String') Like '%" + txtDateRequest.Text + "'";

            }
            bsRequest.Filter = Filter;
            //setRowNumber(GridRequest);
        }
        private void btnNewReq_Click(object sender, EventArgs e)
        {
            Forms.FrmBar FrmBar = new Forms.FrmBar();
            FrmBar.ShowDialog();
            FillRequest();
        }



        private void btnEditReq_Click(object sender, EventArgs e)
        {
            if (GridRequest.SelectedRows.Count > 0)
            {
                int RequestID = int.Parse(GridRequest.Rows[GridRequest.SelectedRows[0].Index].Cells["IDRequest"].Value.ToString());
                if (DAL.CsDALLading.Select("where Tbl_Lading.RequestVehicleID=" + RequestID).Rows.Count > 0)
                {
                    Helper.CsHelper.showMessage("برای این بار حواله ثبت شده است", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Forms.FrmBar FrmBar = new Forms.FrmBar(RequestID);

                FrmBar.ShowDialog();
                btnRefreshRequest_Click(sender, e);
            }

        }

        private void btnDeleteReq_Click(object sender, EventArgs e)
        {

            if (GridRequest.SelectedRows.Count > 0)
            {
                BLL.CsBLLRequestVehicle RequestVehicleItems = new BLL.CsBLLRequestVehicle();
                RequestVehicleItems.ID = int.Parse(GridRequest.Rows[GridRequest.SelectedRows[0].Index].Cells["IDRequest"].Value.ToString());
                if (Helper.CsHelper.showMessage("آیا مطمین به حذف درخواست انتخاب شده هستید؟", "حذف کد " + RequestVehicleItems.ID.ToString(), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                {
                    if (!RequestVehicleItems.Delete())
                    {
                        Helper.CsHelper.showMessage("برای این بار حواله ثبت شده است", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                    FillRequest();

                }
            }
        }
        #endregion


        #region Innings
        BindingSource bsInnings = new BindingSource();
        private void FillInnings()
        {
            gridInnigs.AutoGenerateColumns = false;
            if (cmbInningsGroup.SelectedValue == null)
            {
                return;
            }
            bsInnings.DataSource = DAL.CsDALInnings.select(int.Parse(cmbInningsStatue.SelectedValue == null ? "0" : cmbInningsStatue.SelectedValue.ToString()), int.Parse(cmbInningsGroup.SelectedValue.ToString()));
            gridInnigs.DataSource = bsInnings;
            //setRowNumber(gridInnigs);
        }
        private void cmbInningsStatue_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillInnings();
        }
        private void btnRefreshInnings_Click(object sender, EventArgs e)
        {
            FillInnings();
        }
        private void cmbGroupInnings_SelectedValueChanged(object sender, EventArgs e)
        {
            FillInnings();
        }
        private void FillInningsGroupCombo()
        {

        }
        #endregion

        private string FilterParking()
        {
            try
            {
                string Filter = "1=1 ";
                DateTime dt = new DateTime();
                if (Helper.ConvertDate.PerToGre(txtN_EnterDate.Text) != dt)
                {
                    Filter += " And dbo.MiladiToShamsi( DateEnter) Like '%" + txtN_EnterDate.Text + "'";
                }

                if (Helper.ConvertDate.PerToGre(txtN_ExitDate.Text) != dt)
                {
                    Filter += " And  dbo.MiladiToShamsi(DateExit) Like '%" + txtN_ExitDate.Text + "'";
                }
                if (cmbParkingGroup.SelectedValue != null && cmbParkingGroup.SelectedValue.ToString() != "System.Data.DataRowView" && cmbParkingGroup.SelectedValue.ToString()!="0")
                {
                    Filter += " AND GroupID=" + cmbParkingGroup.SelectedValue.ToString();
                }
                if (txtD_Inttcard.Text != "")
                {
                    Filter += " and InttcardDriver Like N'%" + txtD_Inttcard.Text + "%'";
                }
                if (txtD_Name.Text != "")
                {
                    Filter += " and Name  Like N'%" + txtD_Name.Text + "%'";
                }
                if (txtD_LastName.Text != "")
                {
                    Filter += " and  Family Like N'%" + txtD_LastName.Text + "%'";
                }
                if (txtPelack.Text != "")
                {
                    Filter += " and   Pelak Like N'" + txtPelack.Text + "%'";
                }
                if (txtVehicleTypes.Text != "")
                {
                    Filter += " and  vehicleTypes=N'" + txtVehicleTypes.SelectedItem.ToString() + "'";
                }

                if (cmbUserEnter.SelectedIndex > 0)
                {
                    Filter += " and userEnter=N'" + cmbUserEnter.SelectedItem.ToString() + "'";
                }

                if (CmbUserExit.SelectedIndex > 0)
                {
                    Filter += " and userExit=N'" + CmbUserExit.SelectedItem.ToString() + "'";
                }
                if (!chbCancelParking.Checked)
                {
                    Filter += " and  IsCancel=0";
                }
                if (cmbStatuesParking.SelectedIndex == 1)
                {
                    Filter += " and  isnull(DateExit,0)=0";

                }
                if (cmbStatuesParking.SelectedIndex == 2)
                {
                    Filter += " and  IsInterim=1";

                }
                if (cmbStatuesParking.SelectedIndex == 3)
                {
                    Filter += " and  IsInterim=0";
                    Filter += " and  isnull(DateExit,0)<>0";
                }
                return Filter;

            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain_Load :: FilterParking()>" + ex.Message.ToString());
                Helper.CsHelper.showMessage("در ثبت مقادیر اولیه خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return "";
            }
            
            
        }
        private void btnLading_Click(object sender, EventArgs e)
        {
            new Forms.FrmLading().ShowDialog();
            FillLading();

        }

        private void tabVehicle_Enter(object sender, EventArgs e)
        {
            FillVechile();
        }

        private void tabDriver_Enter(object sender, EventArgs e)
        {
            FillDriver();
        }

        private void tabReqest_Enter(object sender, EventArgs e)
        {
            FillRequest();
        }

        private void tabInnings_Enter(object sender, EventArgs e)
        {
            btnRefreshInnings_Click(sender, e);
        }

        private void tabCompanyList_Enter(object sender, EventArgs e)
        {

        }

        #region Lading

        private void tabLading_Enter(object sender, EventArgs e)
        {
            FillLading();
        }

        private void FillLading()
        {
            dgHavale.AutoGenerateColumns = false;
            bsLading.DataSource = DAL.CsDALLading.Select(" Order by Tbl_Lading.ID desc");
            dgHavale.DataSource = bsLading;

        }
        #endregion

        private void btnEnterInnigs_Click(object sender, EventArgs e)
        {
            new Forms.FrmInningsStatues().ShowDialog();
        }

        private void btnParkingEnter_Click(object sender, EventArgs e)
        {
            new Forms.FrmParkingEnter().ShowDialog();
        }

        private void تنظیماتورودToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Forms.FrmSettingsEnter().ShowDialog();
        }

        private void تعاریفToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Forms.FrmDifantions().ShowDialog();
        }

        private void dgParking_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //if (e.RowIndex >= 0)
            //{
            //    Forms.FrmParking2 FrmParking = new Forms.FrmParking2(int.Parse(dgParking.Rows[e.RowIndex].Cells["ID"].Value.ToString()), int.Parse(dgParking.Rows[e.RowIndex].Cells["IDEnterAndExit"].Value.ToString()));
            //    FrmParking.ShowDialog();
            //    FillParking();
            //}
        }

        Thread trdShowNumber;
        bool ShowNumberIsRun = false;
        bool ShowNumberIsFirst = true;
        void ShowNumber()
        {
            try
            {
                string Rownumber = "0000";
                Rownumber += dtGridInnings.Rows[0].Cells[0].Value.ToString();
                Rownumber = Rownumber.Remove(0, dtGridInnings.Rows[0].Cells[0].Value.ToString().Length);

                WebRequest wq;
                string adrees = "http://" + Helper.Settings.ReadObject(this.Name, "AddressLedInnings").TrimEnd('/');

                wq = WebRequest.Create(adrees + "/?number=" + Rownumber);

                wq.Method = "Get";
                wq.GetResponse();
                wq.Abort();

                ShowNumberIsRun = false;
            }
            catch (Exception exception)
            {
                ShowNumberIsRun = false;
                Helper.CsHelper.writeLog("ShowNumber> " + exception.Message.ToString());
                Helper.CsHelper.showMessage("مسیر تابلو نامعتبر است", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        int CountCallInnnings = 0;
        private void btnShiftNext_Click(object sender, EventArgs e)
        {
            try
            {
                btnRefreshLadings_Click(sender, e);
                if (dtGridInnings.Rows.Count > 0)
                {
                    string Rownumber = "0000";
                    Rownumber += dtGridInnings.Rows[0].Cells[0].Value.ToString();
                    Rownumber = Rownumber.Remove(0, dtGridInnings.Rows[0].Cells[0].Value.ToString().Length);

                    //ShomareGoo.NumberReader nr = new ShomareGoo.NumberReader();
                    //nr.PlayShomareye();
                    //nr.PlayNumberFromResources(int.Parse(Rownumber));
                    //nr.PlayBeBajeye(Settings.Default.NumberBaje);

                    DAL.CsDALSound.Insert(Rownumber, Helper.Settings.ReadObject(this.Name, "NumberBaje") == "" ? "0" : Helper.Settings.ReadObject(this.Name, "NumberBaje"));

                    //

                    if (!ShowNumberIsRun)
                    {
                        try
                        {
                            ShowNumberIsRun = true;

                            if (ShowNumberIsFirst)
                                ShowNumberIsFirst = false;
                            else
                                trdShowNumber.Abort();

                            trdShowNumber = new Thread(new ThreadStart(ShowNumber));
                            trdShowNumber.Start();
                        }
                        catch (Exception ex)
                        {
                            ShowNumberIsRun = false;
                            Helper.CsHelper.writeLog("frmMain :: btnShiftNext_Click() > " + ex.Message.ToString());
                            Helper.CsHelper.showMessage("در عملیات ارسالی تابلو خطایی رخ داده!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }


                    if (CountCallInnnings == 0 || CurentInnings == int.Parse(dtGridInnings.Rows[0].Cells[0].Value.ToString()))
                    {
                        CountCallInnnings++;
                        if (CountCallInnnings >= BLLSettings.CountCallForShowAbsence)
                        {
                            btnAbsent.Enabled = true;
                        }


                    }
                    else
                    {
                        CountCallInnnings = 1;
                        if (CountCallInnnings < BLLSettings.CountCallForShowAbsence)
                        {
                            btnAbsent.Enabled = false;
                        }

                        CurentInnings = int.Parse(dtGridInnings.Rows[0].Cells[0].Value.ToString());

                    }


                }
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("frmMain :: btnShiftNext_Click() > " + ex.Message.ToString());
                Helper.CsHelper.showMessage("خطایی رخ داده!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }




        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            if (GridBar.Rows.Count == 0 || dtGridInnings.Rows.Count == 0)
            {
                return;
            }

            #region Wrongdoer
            BLL.CsBLLWrongdoer wr = new BLL.CsBLLWrongdoer();
            int vehicleID = int.Parse(dtGridInnings.Rows[dtGridInnings.SelectedRows[0].Index].Cells["VehileID"].Value.ToString());
            int DriverID = int.Parse(dtGridInnings.Rows[dtGridInnings.SelectedRows[0].Index].Cells["DriverID"].Value.ToString());

            wr.FillForVehicle(vehicleID);
            wr.VehicleID = DAL.CsDALVehicle.select("", vehicleID);
            if (wr.Statues == 0)
            {
                if (wr.ShowInBaje)
                {
                    new Forms.FrmWrongdoerShow(wr).ShowDialog();

                }
                if (wr.NotInBage)
                {
                    Helper.CsHelper.showMessage("امکان صدور  مجوز برای این ناوگان وجود ندارد", "", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
            }
            wr = new BLL.CsBLLWrongdoer();
            wr.FillForDriver(DriverID);
            wr.DriverID = DAL.CsDALDriver.select("", DriverID, 0);
            if (wr.Statues == 0)
            {
                if (wr.ShowInBaje)
                {
                    new Forms.FrmWrongdoerShow(wr).ShowDialog();

                }
                if (wr.NotInBage)
                {
                    Helper.CsHelper.showMessage("امکان صدور مجوز برای این راننده وجود ندارد", "", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
            } 
            #endregion

            Forms.FrmLaddingJoinInnings frm = new Forms.FrmLaddingJoinInnings(int.Parse(GridBar.Rows[GridBar.SelectedRows[0].Index].Cells["LadingCode"].Value.ToString()), int.Parse(dtGridInnings.Rows[0].Cells["LInningsID"].Value.ToString()), Innings.InningsGroupID.ID);


            frm.ShowDialog();
            CountCallInnnings = 0;
            btnRefreshLadings_Click(sender, e);
        }
    
        private void FillInnigs()
        {
            this.Cursor = Cursors.WaitCursor;
            this.Refresh();
            dtGridInnings.AutoGenerateColumns = false;
            dtGridInnings.DataSource = DAL.CsDALInnings.select(2, Innings.InningsGroupID.ID);
            // dtGridInningsWiate.DataSource = DAL.CsDALInnings.select(5, Innings.InningsGroupID.ID);

            this.Cursor = Cursors.Default;
            this.Refresh();
        }

        private void btnRefreshLadings_Click(object sender, EventArgs e)
        {
            Innings.InningsGroupID.ID = int.Parse(Helper.Settings.ReadObject(this.Name, "GroupInnings") == "" ? "0" : Helper.Settings.ReadObject(this.Name, "GroupInnings"));

            if (Innings.InningsGroupID.ID == 0)
            {
                Helper.CsHelper.showMessage("گروهی برای اعلام بار تعریف نشده است", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            FillInnigs();
            FillBar();

            if (dtGridInnings.Rows.Count == 0 || CurentInnings != int.Parse(dtGridInnings.Rows[0].Cells[0].Value.ToString()))
            {
                if (CountCallInnnings < BLLSettings.CountCallForShowAbsence)
                {
                    btnAbsent.Enabled = false;
                }

            }

        }
        private void FillBar()
        {
            this.Cursor = Cursors.WaitCursor;
            this.Refresh();
            GridBar.AutoGenerateColumns = false;
            bsBar.DataSource = DAL.CsDALRequestVehicle.Select(Innings.InningsGroupID.ID);
            GridBar.DataSource = bsBar.DataSource;
            //  FrmLadingShowBar.GridRequest.DataSource = bsRequest;
            this.Cursor = Cursors.Default;
            this.Refresh();
        }
        private void tabLading1_Enter(object sender, EventArgs e)
        {
            btnRefreshLadings_Click(sender, e);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            BLLSettings.AddressCameraEnter = txtAddressCameraEnter.Text;
            BLLSettings.AddressCameraExit = txtAddressCameraExit.Text;
            BLLSettings.AddressCameraEnter2 = txtAddressCameraEnter2.Text;
            BLLSettings.AddressCameraExit2 = txtAddressCameraExit2.Text;
            BLLSettings.CameraFerem = (int)txtFerem.Value;
            BLLSettings.CancelInnings = chbCancelInnings.Checked;
            BLLSettings.InningsSaveType = (BLL.CsBLLSettings.InningsType)(rdbIsNotEnterSave.Checked ? 1 : 3); //(rdbIsEnterSave.Checked ? 2 : 3));
            BLLSettings.IsInterim = chbIsIntrim.Checked;
            Helper.Settings.SetObjec(this.Name, "ParkingEnterAndExit", rdbEnterAndExit1.Checked ? "1" : "2");
            if (BLLSettings.Save())
                Helper.CsHelper.showMessage("اطلاعات با موفقیت ثبت شد", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                Helper.CsHelper.showMessage("در ثبت اطلاعات مشکلی پیش آمده است", "", MessageBoxButtons.OK, MessageBoxIcon.Stop);

            }
        }

        private void btnSaveSettingsLadinngs_Click(object sender, EventArgs e)
        {
            int c = 0;
            if (txtLedInnings.Text != "" && (txtLedInnings.Text.Split('.').Count() != 4 || !int.TryParse(txtLedInnings.Text.Split('.')[0], out c)))
            {
                Helper.CsHelper.showMessage("آی پی نامعتبر است", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }


            Helper.Settings.SetObjec(this.Name, "GroupInnings", cmbGroupInnings.SelectedValue.ToString());
            Helper.Settings.SetObjec(this.Name, "AddressLedInnings", txtLedInnings.Text);
            Helper.Settings.SetObjec(this.Name, "NumberBaje", txtBaje.Value.ToString());
            Helper.Settings.SetObjec(this.Name, "IsPlaySound", checkPlaySound.Checked ? "1" : "0");
            Helper.Settings.SetObjec(this.Name, "CountShowBar", txtCountShowBar.Value.ToString());
          
            BLLSettings.CountCallForShowAbsence = (int)numCountCallForShowAbsence.Value;
            BLLSettings.Save();
            Helper.CsHelper.showMessage("اطلاعات با موفقیت ثبت شد", "", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void btnShowHavale_Click(object sender, EventArgs e)
        {

            if (dgHavale.Rows.Count == 0)
            {

                return;
            }
            Forms.FrmLaddingJoinInnings frm = new Forms.FrmLaddingJoinInnings(int.Parse(dgHavale.Rows[dgHavale.SelectedRows[0].Index].Cells["IDLading"].Value.ToString()));
            frm.ShowDialog();
        }

      
        private void cmbParkingGroup_SelectedValueChanged(object sender, EventArgs e)
        {
            btnSetParkingFilter_Click(sender, e);

        }

        private void txtD_Inttcard_TextChanged(object sender, EventArgs e)
        {
            btnSetParkingFilter_Click(sender, e);
            

        }

        private void txtVehicleTypes_RightToLeftChanged(object sender, EventArgs e)
        {

        }

   

        private void cmbCompanyList_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterRequest();
        }

        private void cmbGroupSelect_SelectedValueChanged(object sender, EventArgs e)
        {
            FilterRequest();
        }

        private void txtCode_TextChanged(object sender, EventArgs e)
        {
            FilterLading();
        }

        private void FilterLading()
        {
            string Filter = "1=1 ";
            if (txtCode.Text != "")
            {
                Filter += " And CONVERT( ID, 'System.String') ='" + txtCode.Text + "'";
            }
            if (cmbLFromLocation.SelectedItem != null && cmbLFromLocation.SelectedItem.ToString() != "")
            {
                Filter += " And CONVERT( CityFrom, 'System.String') ='" + cmbLFromLocation.SelectedItem.ToString() + "'";
            }
            if (cmbLToLocation.SelectedItem != null && cmbLToLocation.SelectedItem.ToString() != "")
            {
                Filter += " And CONVERT( CityTO, 'System.String') ='" + cmbLToLocation.SelectedItem.ToString() + "'";
            } if (cmbLVechleType.SelectedItem != null && cmbLVechleType.SelectedItem.ToString() != "")
            {
                Filter += " And CONVERT( NameVehicleType, 'System.String') ='" + cmbLVechleType.SelectedItem.ToString() + "'";
            } if (cmbLCapacity.SelectedItem != null && cmbLCapacity.SelectedItem.ToString() != "")
            {
                Filter += " And CONVERT( NameCapacity, 'System.String') ='" + cmbLCapacity.SelectedItem.ToString() + "'";
            }
            for (int i = 0; i < myArea.list.Count; i++)
            {
                if (i==0)
                {
                    Filter += " And ( CONVERT( CityToLocation, 'System.String') like'" + myArea.list[i] + "%' ";
                }
                else
                {
                    Filter += " OR CONVERT( CityToLocation, 'System.String') like'" + myArea.list[i] + "%' ";

                }
                if (i+1==myArea.list.Count)
                {
                    Filter += " ) ";
                }

            }

            bsBar.Filter = Filter;
        }


        private void cmbLFromLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterLading();
        }

        private void btnInningsChangeStatues_Click(object sender, EventArgs e)
        {
            if (gridInnigs.Rows.Count > 0)
            {
                if(int.Parse(gridInnigs.Rows[gridInnigs.SelectedRows[0].Index].Cells["StatueID"].Value.ToString())==(int)Helper.Settings.InningsStatues.EstefadeShode 
                    || int.Parse(gridInnigs.Rows[gridInnigs.SelectedRows[0].Index].Cells["StatueID"].Value.ToString())==(int)Helper.Settings.InningsStatues.Ebtal)
                {
                    //امکان تغییر وضعیت از استفاده شده و ابطال به هیچ وضعیت دیگری نباشد.
                    Helper.CsHelper.showMessage("تغییر وضعیت از «استفاده شده» و «ابطال مجوز و نوبت» به هیچ وضعیت دیگری امکان ندارد.", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                new Forms.FrmInningsStatuesChange(int.Parse(gridInnigs.Rows[gridInnigs.SelectedRows[0].Index].Cells["InningsID"].Value.ToString()), int.Parse(gridInnigs.Rows[gridInnigs.SelectedRows[0].Index].Cells["InningsGroupID"].Value.ToString()), int.Parse(gridInnigs.Rows[gridInnigs.SelectedRows[0].Index].Cells["StatueID"].Value.ToString())).ShowDialog();
                btnRefreshInnings_Click(sender, e);
            }
        }



        #region Sound Service

        Thread trdSoundPlay;
        bool SoundPlayIsRun = false;
        bool SoundPlayIsFirst = true;
        ShomareGoo.NumberReader nr = new ShomareGoo.NumberReader();

        void GetNumbers()
        {
            try
            {
                if (!SoundPlayIsRun)
                {
                    SoundPlayIsRun = true;

                    if (SoundPlayIsFirst)
                        SoundPlayIsFirst = false;
                    else
                        trdSoundPlay.Abort();

                    trdSoundPlay = new Thread(new ThreadStart(SoundPlay));
                    trdSoundPlay.Start();
                }

            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: GetNumbers() " + ex.Message.ToString());
                MessageBox.Show("!در پخش خطایی رخ داده است");
            }
        }

        void SoundPlay()
        {
            //isRun = true;
            string InningsID = "";
            string NumberBaje = "";
            try
            {
                if (!DoesUserHavePermission())
                    return;

                //listBox_ShowData.Items.Clear();

                //  You must stop the dependency before starting a new one.
                //  You must start the dependency when creating a new one.
                SqlDependency.Stop(Helper.CsGeneral.conStrSound);
                SqlDependency.Start(Helper.CsGeneral.conStrSound);

                using (SqlConnection cn = new SqlConnection(Helper.CsGeneral.conStrSound))
                {
                    using (SqlCommand cmd = cn.CreateCommand())
                    {

                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT InningsID,NumberBaje FROM dbo.[Tbl_sound] ";
                        cmd.Notification = null;

                        try
                        {
                            //  creates a new dependency for the SqlCommand
                            SqlDependency dep = new SqlDependency(cmd);
                            //  creates an event handler for the notification of data
                            //      changes in the database.
                            //  NOTE: the following code uses the normal .Net capitalization methods, though
                            //      the forum software seems to change it to lowercase letters
                            dep.OnChange += new OnChangeEventHandler(dep_onchange);

                            cn.Open();
                            bool READ = false;

                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                //cn.Close();

                                while (dr.Read())
                                {
                                    //if (dr.HasRows == false)
                                    //    break;
                                    //listBox_ShowData.Items.Add(dr.GetString(0) + " " + dr.GetString(1));
                                    InningsID = dr.GetString(0);
                                    NumberBaje = dr.GetString(1);
                                    if (!IsSoundLoaded && InningsID != "")
                                    {
                                        if(!READ)
                                        { 
                                            READ = true;

                                            try
                                            {
                                                //ShomareGoo.NumberReader nr = new ShomareGoo.NumberReader();
                                                //nr.PlayShomareye();
                                                //nr.PlayNumberFromResources(int.Parse(InningsID));
                                                //nr.PlayBeBajeye(int.Parse(NumberBaje));
                                                nr.PlayShomareyeSoundPlayer();
                                                nr.PlayNumberFromResourcesSoundPlayer(int.Parse(InningsID));
                                                nr.PlayBeBajeyeSoundPlayer(int.Parse(NumberBaje));
                                                DAL.CsDALSound.Delete(int.Parse(InningsID), int.Parse(NumberBaje));
                                            }
                                            catch (Exception exception)
                                            {
                                                Helper.CsHelper.writeLog("SoundPlay> " + exception.Message.ToString());
                                                DAL.CsDALSound.Delete(int.Parse(InningsID), int.Parse(NumberBaje));
                                                MessageBox.Show("!در پخش خطایی رخ داده است");
                                            }
                                        }
                                    }

                                    //SoundPlay();
                                    //isRun = false;
                                    //return;
                                }
                            }
                            if (READ)
                            {
                                SoundPlay();
                                return;
                            }
                        }
                        catch (Exception exception)
                        {
                            Helper.CsHelper.writeLog("SoundPlay> " + exception.Message.ToString());
                            MessageBox.Show("!در پخش خطایی رخ داده است");
                        }
                    }
                }

            }
            catch (Exception exception)
            {
                Helper.CsHelper.writeLog("SoundPlay> " + exception.Message.ToString());
                MessageBox.Show("!در پخش خطایی رخ داده است");
            }
            finally
            {
                SoundPlayIsRun = false;
                //thread1.Abort();
                //thread1.Suspend();
            }
            //isRun = false;
        }
        private bool DoesUserHavePermission()
        {
            try
            {
                SqlClientPermission clientPermission = new SqlClientPermission(PermissionState.Unrestricted);

                // will throw an error if user does not have permissions
                clientPermission.Demand();

                return true;
            }
            catch
            {
                return false;
            }
        }
        void dep_onchange(object sender, SqlNotificationEventArgs e)
        {
            try
            {
                // this event is run asynchronously so you will need to invoke to run on UI thread(if required).
                //if (this.InvokeRequired)
                //{
                //    //listBox_ShowData.BeginInvoke(new MethodInvoker(GetNumbers));
                //}   
                //else
                GetNumbers();

                // this will remove the event handler since the dependency is only for a single notification
                SqlDependency dep = sender as SqlDependency;

                //  NOTE: the following code uses the normal .Net capitalization methods, though
                //      the forum software seems to change it to lowercase letters
                dep.OnChange -= new OnChangeEventHandler(dep_onchange);
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: dep_onchange() " + ex.Message.ToString());
                MessageBox.Show("!در پخش خطایی رخ داده است");
            }
            
        }
        #endregion

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void txtCompaneID_TextChanged(object sender, EventArgs e)
        {
            FilterCompany();
        }

        private void FilterCompany()
        {
            string Filter = "1=1 ";
            if (txtCompaneID.Text != "")
            {
                Filter += " And CONVERT( Id, 'System.String') ='" + txtCompaneID.Text + "'";
            }
            if (txtCompanyCode.Text != "")
            {
                Filter += " And " + string.Format("CONVERT( OrgCode, 'System.String') LIKE '%{0}%'", Helper.CsHelper.stringToStandard(txtCompanyCode.Text));
            }
            if (txtCompanyName.Text != "")
            {
                Filter += " And " + string.Format("Name LIKE '%{0}%'", Helper.CsHelper.stringToStandard(txtCompanyName.Text));
            }
            if (txtCompanyAdminName.Text != "")
            {
                Filter += " And " + string.Format("CONVERT( Manager, 'System.String') LIKE '%{0}%'", Helper.CsHelper.stringToStandard(txtCompanyAdminName.Text));
            }

            tblTransportCompanyBindingSource.Filter = Filter;

        }

        private void txtSEInttcard_TextChanged(object sender, EventArgs e)
        {

        }

        private string FilterVehicle()
        {
            try
            {
                string filteStr = "";
                string andStr = "";
                if (this.txtSEInttcard.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " Inttcard like '" + txtSEInttcard.Text + "%'";
                    andStr = " and ";
                }
                if (this.txtSEPelak.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " Pelak Like '%" + Helper.CsHelper.toSearchPlaquNumber(this.txtSEPelak.Text) + "%'";
                    andStr = " and ";
                }
                if (this.txtSEPelakseries.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " Pelakseries Like '" + txtSEPelakseries.Text + "%'";
                    andStr = " and ";
                }

                if (this.txtSEModel.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " Model Like '%" + txtSEModel.Text + "%'";
                    andStr = " and ";
                }
                if (this.txtSeOwnerStatuse.SelectedIndex > 0)
                {
                    if (this.txtSeOwnerStatuse.SelectedIndex <= 3)
                    {
                        if (filteStr.Trim().Length > 0)
                        {
                            filteStr = filteStr + " and ";
                        }
                        filteStr = filteStr + " (OwnerStatuse = " + (this.txtSeOwnerStatuse.SelectedIndex - 1) + ") ";
                        andStr = " and ";
                    }
                    else if (this.txtSeOwnerStatuse.SelectedIndex == 5)//غیر ملکی
                    {
                        if (filteStr.Trim().Length > 0)
                        {
                            filteStr = filteStr + " and ";
                        }
                        filteStr = filteStr + " (OwnerStatuse <> 2) ";
                        andStr = " and ";
                    }
                }
                if (this.txtSEvehicleTypes.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " vehicleTypes Like '%" + txtSEvehicleTypes.Text + "%'";
                    andStr = " and ";
                }
                if (this.txtSEDescription.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " Description Like '%" + txtSEDescription.Text + "%'";
                    andStr = " and ";
                }
                if (cmbCompanyTransport.SelectedIndex>0)
                {
                    //filteStr = filteStr + andStr + " TransportCompanyName = '" + cmbCompanyTransport.SelectedItem.ToString()+ "'";


                    Sepehr.Helper.ComboboxItem selectedVeh = (Sepehr.Helper.ComboboxItem)cmbCompanyTransport.SelectedItem;
                    filteStr = filteStr + andStr + " TransportCompanyID = '" + selectedVeh.Value.ToString() + "'";
                    andStr = " and ";
                }
                //this.dv.RowFilter = filteStr;
                return filteStr;
                //setRowNumber(dGV_Vehicle);

            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("105001" + ex.Message.ToString());
                return "";
            }
        }

        private void txtSEInttcardDriver_TextChanged(object sender, EventArgs e)
        {
            FilterDriver();
        }

        private void FilterDriver()
        {
            try
            {
                string filteStr = "";
                string andStr = "";
                if (this.txtSEInttcardDriver.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " Inttcard like '" + txtSEInttcardDriver.Text + "%'";
                    andStr = " and ";
                }
                if (this.txtSEName.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " Name Like '%" + txtSEName.Text + "%'";
                    andStr = " and ";
                }
                if (this.txtSETel.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " Tel Like '%" + txtSETel.Text + "%'";
                    andStr = " and ";
                }
                if (this.txtSEState.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " State Like '%" + txtSEState.Text + "%'";
                    andStr = " and ";
                }
                if (this.txtSECity.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " City Like '%" + txtSECity.Text + "%'";
                    andStr = " and ";
                }
                //if (this.txtSEAddress.Text.Trim() != "")
                //{
                //    filteStr = filteStr + andStr + " Address Like '%" + txtSEAddress.Text + "%'";
                //    andStr = " and ";
                //}
                BsDriver.Filter = filteStr;
                //setRowNumber(dGV_Drivers);

            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("126001 >" + ex.Message.ToString());
            }
        }

        private void btnClearLading_Click(object sender, EventArgs e)
        {
            txtCode.Text = "";
            myArea = new BLL.CsBLLArea();
            cmbLFromLocation.SelectedIndex = cmbLToLocation.SelectedIndex = cmbLVechleType.SelectedIndex = cmbLCapacity.SelectedIndex = -1;
            bsBar.Filter = "";
        }

        private void btnClearRequest_Click(object sender, EventArgs e)
        {
            cmbCompanyList.SelectedIndex = cmbFromLocation.SelectedIndex = cmbToLocation.SelectedIndex = cmbVechleType.SelectedIndex = cmbCapacity.SelectedIndex = -1;
            cmbCompanyList.Text = cmbFromLocation.Text = cmbToLocation.Text = cmbVechleType.Text = cmbCapacity.Text = "";
            txtDateRequest.Text = "13__/__/__";
            cmbGroupSelect.SelectedIndex = 0;
            bsRequest.Filter = "";
        }

        private void txtHavaleID_TextChanged(object sender, EventArgs e)
        {
            FiltrHavale();
        }

        private void FiltrHavale()
        {
            string Filter = "1=1 ";

            if (txtHavaleID.Text != "")
            {
                Filter += " AND CONVERT( ID, 'System.String') Like '" + txtHavaleID.Text + "%'";
            }
            if (txthavaleInningsid.Text != "")
            {
                Filter += " AND CONVERT( InningsID, 'System.String') Like '" + txthavaleInningsid.Text + "%'";
            }
            if (comboHavaleGroup.SelectedValue != null && comboHavaleGroup.SelectedValue.ToString() !="0")
            {
                Filter += " AND CONVERT( InningsGroupID, 'System.String') ='" + comboHavaleGroup.SelectedValue.ToString() + "'";
            }

            DateTime dt = new DateTime();
            if (Helper.ConvertDate.PerToGre(txtHavaleDate.Text) != dt)
            {
                Filter += " And CONVERT( SaveDateShamsi, 'System.String') Like '%" + txtHavaleDate.Text + "'";

            }
            if (txtHavaleCompany.Text != "")
            {
                Filter += " AND CONVERT( NameCompany, 'System.String') Like '%" + txtHavaleCompany.Text + "%'";
            }
            if (txtHavaleDriverName.Text != "")
            {
                Filter += " AND CONVERT( NameDriver, 'System.String') Like '%" + txtHavaleDriverName.Text + "%'";
            }


            bsLading.Filter = Filter;
        }

        private void btnHavaleClear_Click(object sender, EventArgs e)
        {
            txthavaleInningsid.Text = txtHavaleID.Text = txtHavaleDriverName.Text = txtHavaleCompany.Text = "";
            txtHavaleDate.Text = "13__/__/__";
            comboHavaleGroup.SelectedIndex = 0;
        }

        private void btnAbsent_Click(object sender, EventArgs e)
        {

            if (Helper.CsHelper.showMessage("آیا نوبت " + dtGridInnings.Rows[0].Cells[0].Value.ToString() + "به وضعیت 'عدم حضور 'برود؟", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                DAL.CsDALInnings.UpdateStatues(int.Parse(dtGridInnings.Rows[0].Cells[0].Value.ToString()), Innings.InningsGroupID.ID, 5);//عدم حضور
                CountCallInnnings = 0;
                btnRefreshLadings_Click(sender, e);
            }
        }


        private void btnClearParking_Click(object sender, EventArgs e)
        {
            txtD_Inttcard.Text = txtD_Name.Text = txtD_LastName.Text = txtPelack.Text = "";
            txtN_EnterDate.Text = txtN_ExitDate.Text = "13__/__/__";
            //cmbParkingGroup.SelectedValue = 0;
            cmbParkingGroup.SelectedIndex = 0;
            txtVehicleTypes.SelectedIndex = -1;
            cmbStatuesParking.SelectedIndex = 0;
            bsParkingList.Filter = "";
        }


        private void btnRefreshParking_Click(object sender, EventArgs e)
        {
            btnSetParkingFilter_Click(sender, e);
        }

        private void btnRefreshHavale_Click(object sender, EventArgs e)
        {
            FillLading();
        }

        private void btnRefreshRequest_Click(object sender, EventArgs e)
        {
            FillRequest();
        }

        private void btnRefreshVehicle_Click(object sender, EventArgs e)
        {
            FillVechile();
        }

        private void btnRefreshDriver_Click(object sender, EventArgs e)
        {
            FillDriver();
        }

        private void btnShowBar_Click(object sender, EventArgs e)
        {
            new Forms.FrmLadingShowBar().Show();
        }

        private void btnDeleteHavale_Click(object sender, EventArgs e)
        {
            if (dgHavale.SelectedRows.Count > 0 )
            {
                if ((bool)dgHavale.Rows[dgHavale.SelectedRows[0].Index].Cells["IsCancel"].Value)
                {
                    Helper.CsHelper.showMessage("این حواله ابطال شده است", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                int LadingId = int.Parse(dgHavale.Rows[dgHavale.SelectedRows[0].Index].Cells["IDLading"].Value.ToString());
                int innningsId = int.Parse(dgHavale.Rows[dgHavale.SelectedRows[0].Index].Cells["LadingInningsID"].Value.ToString());
                int innningsGroupId = int.Parse(dgHavale.Rows[dgHavale.SelectedRows[0].Index].Cells["LadingInningsGroupID"].Value.ToString());
                new Forms.FrmCancelHaleh(LadingId, innningsId, innningsGroupId).ShowDialog();
                btnRefreshHavale_Click(sender, e);
            }
        }

        private void btnNotActiveRequest_Click(object sender, EventArgs e)
        {

            if (GridRequest.SelectedRows.Count > 0)
            {
                BLL.CsBLLRequestVehicle RequestVehicleItems = new BLL.CsBLLRequestVehicle();
                RequestVehicleItems.ID = int.Parse(GridRequest.Rows[GridRequest.SelectedRows[0].Index].Cells["IDRequest"].Value.ToString());
                if (Helper.CsHelper.showMessage("آیا مطمین به غیر فعال کردن بار انتخاب شده هستید؟", "غیر فعال کردن بار " + RequestVehicleItems.ID.ToString(), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                {
                    RequestVehicleItems.Fill();
                    RequestVehicleItems.IsActive = false;
                    if (!RequestVehicleItems.Update())
                    {
                        Helper.CsHelper.showMessage("در ویرایش خطا به وجود آمد!", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                    FillRequest();

                }
            }
        }

        private void GridRequest_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void rdbCostFixed_CheckedChanged(object sender, EventArgs e)
        {
            txtParkingCost.Enabled = txtInningsCost.Enabled = rdbCostFixed.Checked;
            btnCostFormol.Enabled = rdbCostFormol.Checked;
        }

        private void btnCostFormol_Click(object sender, EventArgs e)
        {
            new Forms.FrmDifantions().ShowDialog();
        }

        private void groupBox12_Enter(object sender, EventArgs e)
        {

        }

        private void tabTeravelTime_Enter(object sender, EventArgs e)
        {
            FillTeravel();
        }

        private void FillTeravel()
        {
            bsTeravelTime.DataSource = DAL.CsDALTeravelTime.select();
            GridTeravelTime.DataSource = bsTeravelTime;
            btnEditTeravel.Enabled = btnAddTeravel.Enabled = false;

        }

        private void btnAddTeravel_Click(object sender, EventArgs e)
        {
            BLL.CsBLLTeravelTime TeravelTime = new BLL.CsBLLTeravelTime();
            TeravelTime.CityFrom = Helper.CsHelper.getValue(this.cmbFromTravel.SelectedItem as Helper.ComboboxItem);
            TeravelTime.CityTo = Helper.CsHelper.getValue(this.cmbToTravel.SelectedItem as Helper.ComboboxItem);
            if (TeravelTime.CityFrom<1 || TeravelTime.CityTo<1 )
            {
                return;
            }
            TeravelTime.Time = (int)txtTimeTeravel.Value;
            if (TeravelTime.Insert())
            {
                FillTeravel();
            }
        }

        private void btnEditTeravel_Click(object sender, EventArgs e)
        {
            BLL.CsBLLTeravelTime TeravelTime = new BLL.CsBLLTeravelTime();
            if (GridTeravelTime.Rows.Count>1)
            {
                return;
            }
            TeravelTime.ID = int.Parse(GridTeravelTime.Rows[0].Cells["IDTeravel"].Value.ToString());
            TeravelTime.CityFrom = Helper.CsHelper.getValue(this.cmbFromTravel.SelectedItem as Helper.ComboboxItem);
            TeravelTime.CityTo = Helper.CsHelper.getValue(this.cmbToTravel.SelectedItem as Helper.ComboboxItem);
            if (TeravelTime.CityFrom < 1 || TeravelTime.CityTo < 1)
            {
                return;
            }
            TeravelTime.Time = (int)txtTimeTeravel.Value;
            if (TeravelTime.Update())
            {
                FillTeravel();
            }
        }

        private void cmbFromTravel_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterTeravleTime();
        }

        private void FilterTeravleTime()
        {
            string Filter = "1=1 ";
            int countfill = 0;
            if (Helper.CsHelper.getValue(cmbFromTravel.SelectedItem as Helper.ComboboxItem)>0)
            {
                Filter += " AND CONVERT( CityFrom, 'System.String') =" + Helper.CsHelper.getValue(cmbFromTravel.SelectedItem as Helper.ComboboxItem);
                countfill++;
            }
            if (Helper.CsHelper.getValue(cmbToTravel.SelectedItem as Helper.ComboboxItem)>0)
            {
                Filter += " AND CONVERT( CityTo, 'System.String') =" + Helper.CsHelper.getValue(cmbToTravel.SelectedItem as Helper.ComboboxItem);
                countfill++;

            }

            bsTeravelTime.Filter = Filter;
            if (GridTeravelTime.Rows.Count>0 && countfill==2)
            {
                btnEditTeravel.Enabled = true;
                btnAddTeravel.Enabled = false;

            }
            if (GridTeravelTime.Rows.Count == 0 && countfill == 2)
            {
                btnAddTeravel.Enabled = true;
                btnEditTeravel.Enabled = false;

            }
            if(countfill>2)
            {
                btnEditTeravel.Enabled = btnAddTeravel.Enabled = false;
            }

        }

        private void btnTeravelClear_Click(object sender, EventArgs e)
        {
            bsTeravelTime.Filter = "";
            cmbFromTravel.Text = cmbToTravel.Text = "";
        }

        private void GridTeravelTime_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            
            if (e.RowIndex>-1)
            {
                string Form =GridTeravelTime.Rows[e.RowIndex].Cells["CityFromTeravelCode"].Value.ToString();
                   
                string To = GridTeravelTime.Rows[e.RowIndex].Cells["CityToTeravelCode"].Value.ToString();
                txtTimeTeravel.Value = decimal.Parse(GridTeravelTime.Rows[e.RowIndex].Cells["Time"].Value.ToString());
                cmbFromTravel.SelectedIndex = Helper.CsHelper.indexOfIntValue(cmbFromTravel, Form);
                cmbToTravel.SelectedIndex = Helper.CsHelper.indexOfIntValue(cmbToTravel, To);
                btnEditTeravel.Enabled = true;
            }
        }

        private void cmbLToLocation_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }

        private void txtInningsNumber_TextChanged(object sender, EventArgs e)
        {
            FilterInnings();
        }

        private void FilterInnings()
        {
            string Filter = "1=1 ";
            if (txtInningsNumber.Text!="")
            {
                Filter += " AND CONVERT( InningsID, 'System.String') Like '" + txtInningsNumber.Text+"%'";
            }
            if (txtDriverintt.Text!="")
            {
                Filter += " AND CONVERT( InttcardDriver, 'System.String') =" + txtDriverintt.Text;

            }
            if (txtVihicleIntt.Text!="")
            {
                Filter += " AND CONVERT( InttcardVehicle, 'System.String') =" + txtVihicleIntt.Text;

            }
            if (txtInningsName.Text != "")
            {
                Filter += " AND CONVERT( Family, 'System.String') =" + txtInningsName.Text;

            }
            DateTime dt = new DateTime();
            if (Helper.ConvertDate.PerToGre(txtDateInnings.Text) != dt)
            {
                Filter += " And CONVERT( SaveDateShamsi, 'System.String') Like '%" + txtDateInnings.Text + "'";
            }
            if (cmbUserInningSave.SelectedIndex > 0)
            {
                Filter += " and CONVERT( UserName, 'System.String')  ='" + cmbUserInningSave.SelectedItem.ToString() + "'";
            }
            bsInnings.Filter = Filter;
            //setRowNumber(gridInnigs);
        }

        private void btnClearInnings_Click(object sender, EventArgs e)
        {
           
            txtInningsNumber.Text = txtInningsName.Text = txtVihicleIntt.Text = txtDriverintt.Text = "";
            txtDateInnings.Text = "13__/__/__";
            cmbUserInningSave.SelectedIndex = 0;
            
        }

        private void btnChangeInningsStatuesAll_Click(object sender, EventArgs e)
        {
            new Forms.FrmInningsStatuesChangesAll().ShowDialog();

            btnRefreshInnings_Click(sender, e);
           
        }

        private void dgParking_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            ////setRowNumber(dgParking);
        }
        public static void setRowNumber(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.HeaderCell.Value = (row.Index + 1).ToString();
            }
            dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.tbl_TransportCompanyTableAdapter.Fill(this.sPBDB_TransportCompany.Tbl_TransportCompany);
        }

        private void btnPrintHavale_Click(object sender, EventArgs e)
        {
            if (dgHavale.SelectedRows.Count>0)
            {
                if ((bool)dgHavale.Rows[dgHavale.SelectedRows[0].Index].Cells["IsCancel"].Value)
                {
                    Helper.CsHelper.showMessage("این حواله ابطال شده است", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                Print(true,int.Parse( dgHavale.Rows[dgHavale.SelectedRows[0].Index].Cells["IDLading"].Value.ToString()));
            }
        }
        private void Print(bool isShow, int idHavale)
        {
            BLL.CsBLLLading LadingItem = new BLL.CsBLLLading();
            LadingItem.ID = idHavale;
            LadingItem.FillID();
            FastReport.Report rep = new FastReport.Report();
            rep.Load(Application.StartupPath + "\\Reports\\ReportLadinng.frx");
            rep.SetParameterValue("DateSave", Helper.ConvertDate.GreToPre( LadingItem.DateSave));
            rep.SetParameterValue("TimeSave", LadingItem.DateSave.ToShortTimeString());
            rep.SetParameterValue("GroupName", LadingItem.InningsID.InningsGroupID.Name);
            rep.SetParameterValue("Inttcard", LadingItem.InningsID.DriverID.Inttcard);
            rep.SetParameterValue("NameDriver", LadingItem.InningsID.DriverID.Name + " " + LadingItem.InningsID.DriverID.Family);
            rep.SetParameterValue("TypeVehicle", LadingItem.InningsID.VehileID.vehicleTypes);
            rep.SetParameterValue("CompanyName", LadingItem.RequestVehicleID.CompanyID.Name);
            rep.SetParameterValue("FromLocation", dgHavale.Rows[dgHavale.SelectedRows[0].Index].Cells["CityFromLocation1"].Value.ToString());
            rep.SetParameterValue("ToLocation", dgHavale.Rows[dgHavale.SelectedRows[0].Index].Cells["CityToLocation1"].Value.ToString());
            rep.SetParameterValue("DocLocation", LadingItem.RequestVehicleID.DocLocation);
            rep.SetParameterValue("DocBar", LadingItem.RequestVehicleID.DocBar);
            rep.SetParameterValue("TypeBar", LadingItem.RequestVehicleID.TypeBar);
            rep.SetParameterValue("WightBar", Convert.ToInt32(LadingItem.RequestVehicleID.WightBar));
            rep.SetParameterValue("Cost", Convert.ToInt32(LadingItem.RequestVehicleID.Cost));
            //rep.SetParameterValue("Pelack", LadingItem.InningsID.VehileID.Pelak + "-" + LadingItem.InningsID.VehileID.Pelakseries);
            rep.SetParameterValue("Pelack", LadingItem.InningsID.VehileID.Pelakseries + "-" + LadingItem.InningsID.VehileID.Pelak);
            BLL.CsBLLUser user = BLL.CsBLLUser.select(LadingItem.UserSave);
            rep.SetParameterValue("UserSave", user.Name+" "+user.LastName);
            rep.SetParameterValue("RequestCode", LadingItem.RequestVehicleID.ID);
            rep.SetParameterValue("LadingID", LadingItem.ID);


            //FastReport.Barcode.BarcodeObject b = rep.FindObject("Barcode1") as FastReport.Barcode.BarcodeObject;

            //b.Text = "0000" + ParkingItems.ParkingID.ID.ToString();
            if (isShow)
            {
                rep.Show();
            }
            else
            {
                rep.Print();
            }

        }

        private void btnPrintParking_Click(object sender, EventArgs e)
        {
            if (dgParking.SelectedRows.Count>0)
            {
                PrintParking(true);
            }
        }
        private void PrintParking(bool isShow)
        {
            int parkingID =int.Parse( dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["ID"].Value.ToString());
            FastReport.Report rep = new FastReport.Report();
            rep.Load(Application.StartupPath + "\\Reports\\ReportParking.frx");
            rep.SetParameterValue("IDParking", parkingID);
            rep.SetParameterValue("ParkingGroup", dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["GroupName1"].Value.ToString());
            rep.SetParameterValue("NameDriver", dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["colName"].Value.ToString()+" "+dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["Family"].Value.ToString());
            rep.SetParameterValue("TypeVehicle", dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["vehicleTypes"].Value.ToString());
            rep.SetParameterValue("Inttcard", dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["InttcardDriver"].Value.ToString());
            rep.SetParameterValue("DateSave", dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["DateEnter"].Value.ToString());
            rep.SetParameterValue("TimeSave",DateTime.Parse( dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["MiladiDateEnter"].Value.ToString()).ToShortTimeString());
            //rep.SetParameterValue("Pelack", dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["Pelak"].Value.ToString() + "-" + dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["Pelakseries"].Value.ToString());
            rep.SetParameterValue("Pelack", dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["Pelakseries"].Value.ToString() + "-" + dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["Pelak"].Value.ToString());
            rep.SetParameterValue("userEnter", dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["userEnter"].Value.ToString());

            FastReport.Barcode.BarcodeObject b = rep.FindObject("Barcode1") as FastReport.Barcode.BarcodeObject;

            string strParking = "0000000000" + parkingID.ToString();
            strParking = strParking.Remove(0, parkingID.ToString().Length);
            b.Text = strParking;
            if (isShow)
            {
                rep.Show();
            }
            else
            {
                rep.Print();
            }

        }

        private void btnPrintInnings_Click_1(object sender, EventArgs e)
        {
            if (gridInnigs.SelectedRows.Count>0)
            {
                printInnings(true);
            }
        }
        private void printInnings(bool isShow)
        {
            BLL.CsBLLInnings InnigsItems = new BLL.CsBLLInnings();
            InnigsItems.InningsID=int.Parse(gridInnigs.Rows[gridInnigs.SelectedRows[0].Index].Cells["InningsID"].Value.ToString());
            InnigsItems.InningsGroupID.ID = int.Parse(gridInnigs.Rows[gridInnigs.SelectedRows[0].Index].Cells["InningsGroupID"].Value.ToString());

            InnigsItems.Fill();
            if (InnigsItems.setInnings(InnigsItems.InningsID, InnigsItems.InningsGroupID.ID))
            {
                //InnigsItems.SetGroupID((int)ParkingItems.ParkingID.VechileID.vehicleTypesCode);
                InnigsItems.InningsGroupID = DAL.CsDALInningsGroup.select(InnigsItems.InningsGroupID.ID);
                FastReport.Report rep = new FastReport.Report();
                rep.Load(Application.StartupPath + "\\Reports\\ReportInnings.frx");
                rep.SetParameterValue("IDParking", InnigsItems.InningsID);
                rep.SetParameterValue("ParkingGroup", InnigsItems.InningsGroupID.Name);
                rep.SetParameterValue("NameDriver", InnigsItems.DriverID.Name+" "+InnigsItems.DriverID.Family);
                rep.SetParameterValue("TypeVehicle", InnigsItems.VehileID.vehicleTypes);
                rep.SetParameterValue("Inttcard", InnigsItems.DriverID.Inttcard);
                rep.SetParameterValue("DateSave", Helper.ConvertDate.GreToPre(InnigsItems.SaveDate));
                rep.SetParameterValue("TimeSave", (InnigsItems.SaveDate).ToShortTimeString());
                //rep.SetParameterValue("Pelack", InnigsItems.VehileID.Pelak + "-" + InnigsItems.VehileID.Pelakseries);
                rep.SetParameterValue("Pelack", InnigsItems.VehileID.Pelakseries + "-" + InnigsItems.VehileID.Pelak);

                BLL.CsBLLUser user = BLL.CsBLLUser.select(InnigsItems.UserSave);
                rep.SetParameterValue("UserSave", user.Name + " " + user.LastName);

                FastReport.Barcode.BarcodeObject b = rep.FindObject("Barcode1") as FastReport.Barcode.BarcodeObject;
                string strInnings = "0000000" + InnigsItems.InningsID.ToString();
                strInnings = strInnings.Remove(0, InnigsItems.InningsID.ToString().Length);

                b.Text = InnigsItems.InningsGroupID.ID.ToString() + strInnings;
                if (isShow)
                {
                    rep.Show();
                }
                else
                {
                    rep.Print();
                }

            }
            else
            {
                Helper.CsHelper.showMessage("برای این راننده نوبتی ثبت نگردیده است", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        void GridView_RowPostPaint(object sender, System.Windows.Forms.DataGridViewRowPostPaintEventArgs e)
        {
            Font s = new Font("2  Yekan", 9);
            DataGridView dgv = (DataGridView)sender;
            using (SolidBrush b = new SolidBrush(dgv.ForeColor))
            {
                e.Graphics.DrawString((e.RowIndex + 1).ToString(System.Globalization.CultureInfo.CurrentUICulture), s, b, dgv.Width - 35, e.RowBounds.Location.Y + 1);
            }
        }

        /////////////////////////////////////
        void GridView_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            SolidBrush b = new SolidBrush(dgv.ForeColor);
            e.Graphics.DrawString("ردیف", dgv.Font, b, dgv.Width - 37, (dgv.RowHeadersWidth / 2) - 17);
        }

        private void cmbLFromLocation_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }

        private void btnTransferPhoto_Click(object sender, EventArgs e)
        {
            DateTime dtFrom=Helper.ConvertDate.PerToGreSafe(txtDateFrom.Text,txtTimeFrom.Text,0,0);
            DateTime dtTo=Helper.ConvertDate.PerToGreSafe(txtDateTo.Text,txtTimeTo.Text,59,999);

            if (dtFrom==new DateTime() || dtTo==new DateTime())
            {
                
                Helper.CsHelper.showMessage("ناریخ ابتدا و انتها کامل وارد نشده است", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            new Forms.FrmTransferPhoto(dtFrom,dtTo,rdbParkingIsExite.Checked).ShowDialog();
        }

        private void btnClearPhoto_Click(object sender, EventArgs e)
        {

            DateTime dtFrom = Helper.ConvertDate.PerToGreSafe(txtDateFrom.Text, txtTimeFrom.Text, 0, 0);
            DateTime dtTo = Helper.ConvertDate.PerToGreSafe(txtDateTo.Text, txtTimeTo.Text, 59, 999);

            if (dtFrom == new DateTime() || dtTo == new DateTime())
            {
                Helper.CsHelper.showMessage("ناریخ ابتدا و انتها کامل وارد نشده است", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            if(Helper.CsHelper.showMessage("با این عملیات تمام عکس های ورود و خروج پارکینگ حذف خواهد شد.\n آبا مطمین هستید؟","توجه",MessageBoxButtons.YesNo,MessageBoxIcon.Stop)==DialogResult.Yes)
            {
                DAL.CsDALParkingEnterAndExit.UpdateClearPhotoForTime(dtFrom, dtTo,rdbParkingIsExite.Checked);
                Helper.CsHelper.showMessage("عملیات با موفقیت انجام شد","",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }

        private void dgHavale_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
          
        }

        private void tabPageUser_Enter(object sender, EventArgs e)
        {
            try
            {
                this.tbl_userTableAdapter.Connection.ConnectionString = Helper.CsGeneral.conStrSepehrDB;
                this.tbl_userTableAdapter.Fill(this.sSDBS_User.Tbl_user);
                // TODO: This line of code loads data into the 'sSDBS_PrintFile.Tbl_PrintFile' table. You can move, or remove it, as needed.
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("199001>" + ex.Message.ToString());
            }
        }

        private void btnAddUser_Click_1(object sender, EventArgs e)
        {
            FrmUser frmUser1 = new FrmUser();
            frmUser1.formMode = Helper.CsType.FormMode.ADD;
            if (frmUser1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.tbl_userTableAdapter.Fill(this.sSDBS_User.Tbl_user);
            }
        }

        private void btnEditUser_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dGV_User.SelectedRows.Count <= 0)
                {
                    return;
                }
                DataGridViewRow dgvr = new DataGridViewRow();
                dgvr = this.dGV_User.SelectedRows[0];
                Int64 idEdit = Int64.Parse(dgvr.Cells["US_Id"].Value.ToString());
                FrmUser frmUser1 = new FrmUser();
                frmUser1.formMode = Helper.CsType.FormMode.EDIT;
                frmUser1.setForm(idEdit);
                if (frmUser1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.tbl_userTableAdapter.Fill(this.sSDBS_User.Tbl_user);
                    //int varIndex = this.tblTravelFinanceBindingSource1.Find("Id", idEdit);
                    //if ((varIndex <= this.dGV_User.Rows.Count) && (varIndex >= 0))
                    //    this.dGV_User.Rows[varIndex].Selected = true;

                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnDelUser_Click(object sender, EventArgs e)
        {

        }

        private void btnAcssesLevel_Click(object sender, EventArgs e)
        {
            new Forms.FrmAcssesLevelUser().ShowDialog();

        }

        private void txtPelack_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }

        private void txtPelack_TextChanged(object sender, EventArgs e)
        {
            if (this.txtPelack.Text.Length == 6)
            {
                this.txtPelack.Text = Helper.CsHelper.toStandardPlaquNumber(this.txtPelack.Text, true);
            }
            FilterParking();
        }

        private void btnCancelParking_Click(object sender, EventArgs e)
        {
            int IdEnterAndExit=int.Parse(dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["IDEnterAndExit"].Value.ToString());
            int Id=int.Parse(dgParking.Rows[dgParking.SelectedRows[0].Index].Cells["ID"].Value.ToString());
            if (Helper.CsHelper.showMessage("آیا مطمِئن به ابطال قبض شماره "+Id+" هستید؟ ","",MessageBoxButtons.YesNo,MessageBoxIcon.Hand)==DialogResult.Yes)
            {
                DAL.CsDALParkingEnterAndExit.CancelParking(IdEnterAndExit);
                FillParking();
            }
        }

        private void dgParking_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            double sum=0;
            for (int i = 0; i < dgParking.Rows.Count; i++)
			{
			    if(dgParking.Rows[i].Cells["CostParking"].Value.ToString()!="")
                    sum+=double.Parse(dgParking.Rows[i].Cells["CostParking"].Value.ToString());
			}

            txtSumCostParking.Text = sum.ToString("F0");
            
        }

        private void comboHavaleGroup_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnInsertWrongdoer_Click(object sender, EventArgs e)
        {
            Forms.FrmWrongdoer frmWrongdoer = new Forms.FrmWrongdoer();
            frmWrongdoer.ShowDialog();
            FillWrongdoer();
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        BLL.CsBLLArea myArea = new BLL.CsBLLArea();
        private void btnShowMAp_Click(object sender, EventArgs e)
        {
            try
            {
                FrmIranMap frmIranMap1 = new FrmIranMap();
                
                frmIranMap1.setAreaList(myArea);
                if (frmIranMap1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    myArea = frmIranMap1.getArea();
                    // در متغیر خروجی لیست کد استانهای انتخاب شده وجود دارد
                    FilterLading();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnInningsReport_Click(object sender, EventArgs e)
        {
            new Forms.Reports.FrmInningsReport().ShowDialog();
        }

        private void btnReportParking_Click(object sender, EventArgs e)
        {
            new Forms.Reports.FrmParkingReport().ShowDialog();
        }

        private void btnRefreshWrongdoer_Click(object sender, EventArgs e)
        {
            FillWrongdoer();
        }

        BindingSource bsWrongdoer = new BindingSource();
        private void FillWrongdoer()
        {
            dgWrongdoer.AutoGenerateColumns = false;
            bsWrongdoer.DataSource = DAL.CsDALWrongdoer.SelectAll("");
            dgWrongdoer.DataSource = bsWrongdoer;

        }

        private void tabWrongdoer_Enter(object sender, EventArgs e)
        {
            FillWrongdoer();
        }

        private void btnEditWrongdoer_Click(object sender, EventArgs e)
        {
            if (dgWrongdoer.SelectedRows.Count>0)
            {
                int id=int.Parse(dgWrongdoer.Rows[dgWrongdoer.SelectedRows[0].Index].Cells["WrongdoerID"].Value.ToString());
                new Forms.FrmWrongdoer(id).ShowDialog();
                btnRefreshWrongdoer_Click(sender, e);
            }
        }

        private void btnRelease_Click(object sender, EventArgs e)
        {
            if(Helper.CsHelper.showMessage("آیا مطمئن به آزاد سازی متخلف جاری هستید؟","",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes)
            {
                int id=int.Parse(dgWrongdoer.Rows[dgWrongdoer.SelectedRows[0].Index].Cells["WrongdoerID"].Value.ToString());
                DAL.CsDALWrongdoer.UpdateStatues(id, 1);
                btnRefreshWrongdoer_Click(sender, e);
            }
        }

        private void txtD_InttcartWrongdoer_TextChanged(object sender, EventArgs e)
        {
            FilterWrongdoer();
        }

        private void FilterWrongdoer()
        {
            try
            {
                string Filter = "1=1 ";
                DateTime dt = new DateTime();
                if (Helper.ConvertDate.PerToGre(txtDateSaveWrongdoer.Text) != dt)
                {
                    Filter += " And CONVERT( SaveDateShamsi, 'System.String') Like '%" + txtDateSaveWrongdoer.Text + "'";
                }

                
              
                if (txtD_InttcartWrongdoer.Text != "")
                {
                    Filter += " and CONVERT( InttcardDriver, 'System.String') Like '%" + txtD_InttcartWrongdoer.Text + "%'";
                }
                if (txtV_InttcartWrongdoer.Text != "")
                {
                    Filter += " and CONVERT( VehicleInttcard, 'System.String') Like '%" + txtV_InttcartWrongdoer.Text + "%'";
                }
              
                if (txtD_NameWrongdoer.Text != "")
                {
                    Filter += " and CONVERT( Family, 'System.String') Like'%" + txtD_NameWrongdoer.Text + "%'";
                }
                if (txtPelakWrongdoer.Text != "")
                {
                    Filter += " and CONVERT( Pelak, 'System.String') Like'%" + txtPelakWrongdoer.Text + "%'";
                }


                if (cmbStatuesWrongdoer.SelectedIndex > 0)
                {
                    Filter += " and CONVERT( Statues, 'System.String')  ='" + (cmbStatuesWrongdoer.SelectedIndex - 1).ToString() + "'";
                }

              
                bsWrongdoer.Filter = Filter;

            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain_Load :: FilterWrongdoer()>" + ex.Message.ToString());
            }
        }

        private void txtPelakWrongdoer_TextChanged(object sender, EventArgs e)
        {
            if (this.txtPelakWrongdoer.Text.Length == 6)
            {
                this.txtPelakWrongdoer.Text = Helper.CsHelper.toStandardPlaquNumber(this.txtPelakWrongdoer.Text, true);
            }
            FilterWrongdoer();
        }

        private void btnClearWrongdoer_Click(object sender, EventArgs e)
        {
            bsWrongdoer.Filter = "";
            cmbStatuesWrongdoer.SelectedIndex = 0;
            txtD_InttcartWrongdoer.Text = txtD_NameWrongdoer.Text = txtV_InttcartWrongdoer.Text = txtPelakWrongdoer.Text = "";
            txtDateSaveWrongdoer.Text = "13__/__/__";
        }

        private void txtPelakWrongdoer_TextAlignChanged(object sender, EventArgs e)
        {

        }
        private void FillParKing_Innings_Vehicle()
        {
            DataGridViewComboBoxColumn cmbParkigGRoup = (DataGridViewComboBoxColumn)gridParKing_Innings_Vehicle.Columns["ComboParkingGroup"];
            cmbParkigGRoup.DataSource = DAL.CsDALParkingGroup.SelectAll(" where IsActive=1");
            cmbParkigGRoup.DisplayMember = "Name";
            cmbParkigGRoup.ValueMember = "ID";

            DataGridViewComboBoxColumn cmbInningsGroup = (DataGridViewComboBoxColumn)gridParKing_Innings_Vehicle.Columns["ComboInningsGroup"];
            cmbInningsGroup.DataSource = DAL.CsDALInningsGroup.SelectAll(" where Active=1");
            cmbInningsGroup.DisplayMember = "Name";
            cmbInningsGroup.ValueMember = "ID";

            gridParKing_Innings_Vehicle.AutoGenerateColumns = false;
            gridParKing_Innings_Vehicle.DataSource = DAL.CsDALParKing_Innings_Vehicle.select();


        }

        private void FillParkingGroup()
        {
            gridParkingGroup.AutoGenerateColumns = false;
            gridParkingGroup.DataSource = DAL.CsDALParkingGroup.SelectAll("");

        }

        private void BtnSaveGrid_p_i_v_Click(object sender, EventArgs e)
        {
            gridParKing_Innings_Vehicle.EndEdit();
            this.Cursor = Cursors.WaitCursor;
            this.Refresh();
            for (int i = 0; i < gridParKing_Innings_Vehicle.Rows.Count; i++)
            {
                int VehicleTypeID;
                int pg, Ig;
                if (gridParKing_Innings_Vehicle.Rows[i].Cells["ComboInningsGroup"].Value != null || gridParKing_Innings_Vehicle.Rows[i].Cells["ComboParkingGroup"].Value != null)
                {
                    int.TryParse(gridParKing_Innings_Vehicle.Rows[i].Cells["IDvehicleTypes"].Value.ToString(), out VehicleTypeID);

                    int.TryParse(gridParKing_Innings_Vehicle.Rows[i].Cells["ComboParkingGroup"].Value.ToString(), out pg);
                    int.TryParse(gridParKing_Innings_Vehicle.Rows[i].Cells["ComboInningsGroup"].Value.ToString(), out Ig);

                    DAL.CsDALParKing_Innings_Vehicle.insert(VehicleTypeID, pg, Ig);
                }

            }
            this.Cursor = Cursors.Default;
            this.Refresh();
        }

        private void btnAddp_Click(object sender, EventArgs e)
        {
            ParkingGroup = new BLL.CsBLLParkingGroup();
            txtp_Group.Text = txtParkingCostInDay.Text = txtParkingCostInHours.Text = "";
            panelDefanctionGroup.Enabled = true;
        }

        private void btnEditp_Click(object sender, EventArgs e)
        {
            panelDefanctionGroup.Enabled = true;
        }

        private void btnSavep_Click(object sender, EventArgs e)
        {
            ParkingGroup.Name = txtp_Group.Text;
            ParkingGroup.CostInHours = long.Parse(txtParkingCostInHours.TextValue.ToString());
            ParkingGroup.CostInDays = long.Parse(txtParkingCostInDay.TextValue.ToString());
            ParkingGroup.IsActive = chbActiveParking.Checked;

            if (ParkingGroup.ID > 0)
            {
                DAL.CsDALParkingGroup.Update(ParkingGroup);
            }
            else
            {
                DAL.CsDALParkingGroup.Insert(ParkingGroup);
            }
            FillParkingGroup();
            panelDefanctionGroup.Enabled = false;
        }

        private void gridParkingGroup_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                ParkingGroup.ID = int.Parse(gridParkingGroup.Rows[e.RowIndex].Cells["ColID"].Value.ToString());
                txtp_Group.Text = gridParkingGroup.Rows[e.RowIndex].Cells["pName"].Value.ToString();
                txtParkingCostInDay.Text = gridParkingGroup.Rows[e.RowIndex].Cells["CostInDays"].Value.ToString();
                txtParkingCostInHours.Text = gridParkingGroup.Rows[e.RowIndex].Cells["CostInHours"].Value.ToString();
                chbActiveParking.Checked = (bool)gridParkingGroup.Rows[e.RowIndex].Cells["pIsActive"].Value;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            panelDefanctionGroup.Enabled = false;

        }


        private void btnOk_Click(object sender, EventArgs e)
        {
           
                BLL.CsBLLCompany myCompany = new BLL.CsBLLCompany();

                myCompany.Id = this.editId;
                myCompany.SazmanCode = (Int64)this.txtSazmanCode.TextValue;
                myCompany.Name = this.txtName.Text;
                myCompany.Manager = this.txtManager.Text;
                myCompany.NationalCode = this.txtNationalCode.Text;
                myCompany.EconomyCode = this.txtEconomyCode.Text;
                myCompany.PostalCode = this.txtPostalCode.Text;
                myCompany.Tel = this.txtTel.Text;
                myCompany.Fax = this.txtFax.Text;
                myCompany.Address = this.txtAddress.Text;
                if (editId <= 0)
                {
                    if (myCompany.insert() > 0)
                    {
                        this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    }
                    else
                    {
                        Helper.CsHelper.showMessage("در ثبت اطلاعات خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    if (myCompany.edit())
                    {
                        this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    }
                    else
                    {
                        Helper.CsHelper.showMessage("در ویرایش اطلاعات خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

            
        }

        private void tabCompanyInfo_Enter(object sender, EventArgs e)
        {
            try
            {
                BLL.CsBLLCompany myCompany = new BLL.CsBLLCompany();
                myCompany = BLL.CsBLLCompany.select();
                if (myCompany != null)
                {
                    this.editId = myCompany.Id;
                    this.txtSazmanCode.Text = myCompany.SazmanCode.ToString();
                    this.txtName.Text = myCompany.Name;
                    this.txtManager.Text = myCompany.Manager;
                    this.txtNationalCode.Text = myCompany.NationalCode;
                    this.txtEconomyCode.Text = myCompany.EconomyCode;
                    this.txtPostalCode.Text = myCompany.PostalCode;
                    this.txtTel.Text = myCompany.Tel;
                    this.txtFax.Text = myCompany.Fax;
                    this.txtAddress.Text = myCompany.Address;
                   
                }
               

            }
            catch (Exception ex)
            {
                Helper.CsHelper.showMessage("در بارگذاری اطلاعات خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               
            }
        }

      
        private void tabDefantionParkingGroup_Enter(object sender, EventArgs e)
        {
            FillParkingGroup();

        }

        private void tabDefantionJoin_Enter(object sender, EventArgs e)
        {
            FillParKing_Innings_Vehicle();

        }

        private void txtHavaleDriverName_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }

        private void txtD_NameWrongdoer_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }

        private void txtInningsName_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }

        private void tabDefanction_Enter(object sender, EventArgs e)
        {
            tabCompanyInfo_Enter(sender, e);
        }

        #region tabSettingsEnter
        private void tabSettingsEnter_Enter(object sender, EventArgs e)
        {
            cmbLevel.SelectedIndex = int.Parse(Helper.Settings.ReadObject("FrmMain", "LevelAcsses") == "" ? "0" : Helper.Settings.ReadObject("FrmMain", "LevelAcsses"));
            cmbFormsCode.SelectedIndex = int.Parse(Helper.Settings.ReadObject("FrmMain", "FormsCode") == "" ? "0" : Helper.Settings.ReadObject("FrmMain", "FormsCode"));
        }

        private void btnSaveSettingEnter_Click(object sender, EventArgs e)
        {
            Helper.Settings.SetObjec("FrmMain", "LevelAcsses", cmbLevel.SelectedIndex.ToString());
            Helper.Settings.SetObjec("FrmMain", "FormsCode", cmbFormsCode.SelectedIndex.ToString());

            Settings.Default.Save();

        }

        #endregion

        private void btnCopyBar_Click(object sender, EventArgs e)
        {
            if (GridRequest.SelectedRows.Count > 0)
            {
                int RequestID = int.Parse(GridRequest.Rows[GridRequest.SelectedRows[0].Index].Cells["IDRequest"].Value.ToString());
                if (DAL.CsDALLading.Select("where Tbl_Lading.RequestVehicleID=" + RequestID).Rows.Count > 0)
                {
                    Helper.CsHelper.showMessage("برای این بار حواله ثبت شده است", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Forms.FrmBar FrmBar = new Forms.FrmBar(RequestID);
                FrmBar.IsCopy = true;
                FrmBar.ShowDialog();
                btnRefreshRequest_Click(sender, e);
            }
        }

        private void cmbCompanyTransport_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSetVehicleFilter_Click(sender, e);
        }

        private void cmbCompanyTransport_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = Helper.CsHelper.charToStandard(e.KeyChar);
        }

        private void btnClearVehicle_Click(object sender, EventArgs e)
        {
            txtSEInttcard.Text = txtSEPelak.Text = txtSEPelakseries.Text = txtSEModel.Text = txtSEvehicleTypes.Text = txtSEDescription.Text = txtSEvehicleTypes.Text = "";
            txtSeOwnerStatuse.SelectedIndex = -1;
            cmbCompanyTransport.SelectedIndex = -1;
        }

        private void btnReportStatuLog_Click(object sender, EventArgs e)
        {
            new Forms.Reports.FrmInningsLogReport(int.Parse(gridInnigs.Rows[gridInnigs.SelectedRows[0].Index].Cells["InningsID"].Value.ToString()), int.Parse(gridInnigs.Rows[gridInnigs.SelectedRows[0].Index].Cells["InningsGroupID"].Value.ToString())).ShowDialog();
        }

        private void rangeParking_RangeChanged(object sender, EventArgs e)
        {
            FillParking();
        }

        private void btnShowFirstDriver_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_str1 = Driver_Selectquery;
                Driver_rowOffset = 1;
                Int32 tmp = Driver_rowLimit + 1;
                string filter_str2 = " where (rownumber>=" + Driver_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                dGV_Drivers.DataSource = dal.select("SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Family ,Name ) AS rownumber,* from Tbl_Drivers " + filter_str1 + ") as Tbl_Drivers " + filter_str2);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowFirstDriver_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات رانندگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledDriverBtn();
            }

        }

        private void btnShowPriviousDriver_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_str1 = Driver_Selectquery;
                Driver_rowOffset = Driver_rowOffset - Driver_rowLimit;
                if (Driver_rowOffset < 1)
                    Driver_rowOffset = 1;
                Int32 tmp = Driver_rowOffset + Driver_rowLimit;
                string filter_str2 = " where (rownumber>=" + Driver_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                dGV_Drivers.DataSource = dal.select("SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Family ,Name ) AS rownumber,* from Tbl_Drivers " + filter_str1 + ") as Tbl_Drivers " + filter_str2);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowPriviousDriver_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات رانندگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledDriverBtn();
            }

        }

        private void btnShowNextDriver_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_str1 = Driver_Selectquery;
                Driver_rowOffset = Driver_rowOffset + Driver_rowLimit;
                Int32 tmp = Driver_rowOffset + Driver_rowLimit;
                if (tmp > Driver_rowCount)
                    Driver_rowOffset = Driver_rowCount - Driver_rowLimit + 1;

                string filter_str2 = " where (rownumber>=" + Driver_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                dGV_Drivers.DataSource = dal.select("SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Family ,Name ) AS rownumber,* from Tbl_Drivers " + filter_str1 + ") as Tbl_Drivers " + filter_str2);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowNextDriver_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات رانندگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledDriverBtn();
            }

        }

        private void btnShowLastDriver_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_str1 = Driver_Selectquery;
                Driver_rowOffset = Driver_rowCount - Driver_rowLimit + 1;
                if (Driver_rowOffset <= 0)
                    Driver_rowOffset = 1;
                Int32 tmp = Driver_rowOffset + Driver_rowLimit;
                string filter_str2 = " where (rownumber>=" + Driver_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                dGV_Drivers.DataSource = dal.select("SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Family ,Name ) AS rownumber,* from Tbl_Drivers " + filter_str1 + ") as Tbl_Drivers " + filter_str2);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowLastDriver_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات رانندگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledDriverBtn();
            }
        }
        public void checkEnabledVehicleBtn()
        {
            try
            {
                btnShowNextVehicle.Visible = true;
                btnShowFirstVehicle.Visible = true;
                btnShowLastVehicle.Visible = true;
                btnShowPriviousVehicle.Visible = true;
                if (Vehicle_rowOffset <= 1)
                {

                    btnShowFirstVehicle.Visible = false;
                    btnShowPriviousVehicle.Visible = false;
                }
                if ((Vehicle_rowOffset + Vehicle_rowLimit) > Vehicle_rowCount)
                {
                    btnShowNextVehicle.Visible = false;
                    btnShowLastVehicle.Visible = false;
                }
                this.lblFromRecord_Vehicle.Text = Vehicle_rowOffset.ToString();
                Int32 tmp = Vehicle_rowOffset + this.dGV_Vehicle.RowCount - 1;
                if (tmp < 0)
                    tmp = 0;
                this.lbltoRecord_Vehicle.Text = (tmp).ToString();
                this.lblAllRecord_Vehicle.Text = Vehicle_rowCount.ToString();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: getVehicleFilter()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("در نمایش اطلاعات خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }
        public void checkEnabledDriverBtn()
        {
            try
            {
                btnShowNextDriver.Visible = true;
                btnShowFirstDriver.Visible = true;
                btnShowLastDriver.Visible = true;
                btnShowPriviousDriver.Visible = true;
                if (Driver_rowOffset <= 1)
                {

                    btnShowFirstDriver.Visible = false;
                    btnShowPriviousDriver.Visible = false;
                }
                if ((Driver_rowOffset + Driver_rowLimit) > Driver_rowCount)
                {
                    btnShowNextDriver.Visible = false;
                    btnShowLastDriver.Visible = false;
                }
                this.lblFromRecord_Driver.Text = Driver_rowOffset.ToString();
                Int32 tmp = Driver_rowOffset + this.dGV_Drivers.RowCount - 1;
                if (tmp < 0)
                    tmp = 0;
                this.lbltoRecord_Driver.Text = (tmp).ToString();
                this.lblAllRecord_Driver.Text = Driver_rowCount.ToString();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: getDriverFilter()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("در نمایش اطلاعات خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }
        private void btnSetDriverFilter_Click(object sender, EventArgs e)
        {
            try
            {
                Driver_rowOffset = 1;
                string filter_str1 = getDriverFilter();
                string queryStr = "Select count(*) AllRowCount from Tbl_Drivers ";
                if (filter_str1.Trim() != "")
                    filter_str1 = " where " + filter_str1;
                queryStr = queryStr + filter_str1;
                Driver_Selectquery = filter_str1;
                SqlConnection con = new SqlConnection(Helper.CsGeneral.conStrSepehrDB);
                SqlCommand cmd = new SqlCommand(queryStr, con);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                Driver_rowCount = Int32.Parse(cmd.ExecuteScalar().ToString());
                string filter_str2 = " where (rownumber>=" + Driver_rowOffset + ") and (rownumber<" + (Driver_rowOffset + Driver_rowLimit).ToString() + ") ";
                //this.tbl_DriversTableAdapter.Adapter.SelectCommand.CommandText = "SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Family ,Name ) AS rownumber,* from Tbl_Drivers " + filter_str1 + ") as Tbl_Drivers " + filter_str2;
                //this.tbl_DriversTableAdapter.Fill(this.sSDB_Drivers.Tbl_Drivers);
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                dGV_Drivers.DataSource = dal.select("SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Family ,Name ) AS rownumber,* from Tbl_Drivers " + filter_str1 + ") as Tbl_Drivers " + filter_str2);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: getDriverFilter()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات رانندگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledDriverBtn();
            }
        }
        private string getDriverFilter()
        {
            try
            {
                string filteStr = "";
                string andStr = "";
                if (this.txtSEInttcardDriver.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " Inttcard like '" + txtSEInttcardDriver.Text + "%'";
                    andStr = " and ";
                }
                if (this.txtSEName.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " Name Like '%" + txtSEName.Text + "%'";
                    andStr = " and ";
                }
                if (this.txtSETel.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " Tel Like '%" + txtSETel.Text + "%'";
                    andStr = " and ";
                }
                if (this.txtSEState.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " State Like '%" + txtSEState.Text + "%'";
                    andStr = " and ";
                }
                if (this.txtSECity.Text.Trim() != "")
                {
                    filteStr = filteStr + andStr + " City Like '%" + txtSECity.Text + "%'";
                    andStr = " and ";
                }
                //if (this.txtSEAddress.Text.Trim() != "")
                //{
                //    filteStr = filteStr + andStr + " Address Like '%" + txtSEAddress.Text + "%'";
                //    andStr = " and ";
                //}
                return filteStr;
                //setRowNumber(dGV_Drivers);

            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: getDriverFilter()> >" + ex.Message.ToString());
                return "";
            }
        }

        private void btnSetVehicleFilter_Click(object sender, EventArgs e)
        {
            try
            {
                Vehicle_rowOffset = 1;
                string filter_str1 = FilterVehicle();
                string queryStr = "Select count(*) AllRowCount from Tbl_Vehicle ";
                if (filter_str1.Trim() != "")
                    filter_str1 = " where " + filter_str1;
                queryStr = queryStr + filter_str1;
                Vehicle_Selectquery = filter_str1;
                SqlConnection con = new SqlConnection(Helper.CsGeneral.conStrSepehrDB);
                SqlCommand cmd = new SqlCommand(queryStr, con);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                Vehicle_rowCount = Int32.Parse(cmd.ExecuteScalar().ToString());
                string filter_str2 = " where (rownumber>=" + Driver_rowOffset + ") and (rownumber<" + (Driver_rowOffset + Driver_rowLimit).ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
               dGV_Vehicle.DataSource=  dal.select("SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Id) AS rownumber,* from Tbl_Vehicle " + filter_str1 + ") as Tbl_Vehicle " + filter_str2);
               dal.disConnect();
                //this.tbl_DriversTableAdapter.Adapter.SelectCommand.CommandText = "SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Id) AS rownumber,* from Tbl_Vehicle " + filter_str1 + ") as Tbl_Vehicle " + filter_str2;
                //this.tbl_DriversTableAdapter.Fill(this.sSDB_Drivers.Tbl_Drivers);
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: getDriverFilter()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات رانندگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledVehicleBtn();
            }
        }

        private void btnShowFirstVehicle_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_str1 = Vehicle_Selectquery;
                Vehicle_rowOffset = 1;
                Int32 tmp = Vehicle_rowLimit + 1;
                string filter_str2 = " where (rownumber>=" + Vehicle_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                dGV_Vehicle.DataSource = dal.select("SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Id) AS rownumber,* from Tbl_Vehicle " + filter_str1 + ") as Tbl_Vehicle " + filter_str2);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowFirstVehicle_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات ناوگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledVehicleBtn();
            }
        }

        private void btnShowPriviousVehicle_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_str1 = Vehicle_Selectquery;
                Vehicle_rowOffset = Vehicle_rowOffset - Vehicle_rowLimit;
                if (Vehicle_rowOffset < 1)
                    Vehicle_rowOffset = 1;
                Int32 tmp = Vehicle_rowOffset + Vehicle_rowLimit;
                string filter_str2 = " where (rownumber>=" + Vehicle_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                dGV_Vehicle.DataSource = dal.select("SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Id) AS rownumber,* from Tbl_Vehicle " + filter_str1 + ") as Tbl_Vehicle " + filter_str2);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowPriviousVehicle_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات ناوگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledVehicleBtn();
            }
        }

        private void btnShowNextVehicle_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_str1 = Vehicle_Selectquery;
                Vehicle_rowOffset = Vehicle_rowOffset + Vehicle_rowLimit;
                Int32 tmp = Vehicle_rowOffset + Vehicle_rowLimit;
                if (tmp > Vehicle_rowCount)
                    Vehicle_rowOffset = Vehicle_rowCount - Vehicle_rowLimit + 1;

                string filter_str2 = " where (rownumber>=" + Driver_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                dGV_Vehicle.DataSource = dal.select("SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Id) AS rownumber,* from Tbl_Vehicle " + filter_str1 + ") as Tbl_Vehicle " + filter_str2);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowNextVehicle_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات ناوگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledVehicleBtn();
            }
        }

        private void btnShowLastVehicle_Click(object sender, EventArgs e)
        {

            try
            {
                string filter_str1 = Vehicle_Selectquery;
                Vehicle_rowOffset = Vehicle_rowCount - Driver_rowLimit + 1;
                if (Vehicle_rowOffset <= 0)
                    Vehicle_rowOffset = 1;
                Int32 tmp = Vehicle_rowOffset + Vehicle_rowLimit;
                string filter_str2 = " where (rownumber>=" + Vehicle_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                dGV_Vehicle.DataSource = dal.select("SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Id) AS rownumber,* from Tbl_Vehicle " + filter_str1 + ") as Tbl_Vehicle " + filter_str2);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowLastVehicle_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات ناوگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledVehicleBtn();
            }
        }

        private void btnSetParkingFilter_Click(object sender, EventArgs e)
        {
            try
            {
                Parking_rowOffset = 1;
                string filter_str1 = FilterParking();
                string queryStr = "Select count(*) AllRowCount from Tbl_Parking INNER JOIN Tbl_ParkingEnterAndExit ON Tbl_Parking.ID = Tbl_ParkingEnterAndExit.ParkingID  ";
                if (filter_str1.Trim() != "")
                    filter_str1 = " where " + filter_str1;
                queryStr = queryStr + filter_str1;
                Parking_Selectquery = filter_str1;
                SqlConnection con = new SqlConnection(Helper.CsGeneral.conStrSepehrDB);
                SqlCommand cmd = new SqlCommand(queryStr, con);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                Parking_rowCount = Int32.Parse(cmd.ExecuteScalar().ToString());
                string filter_str2 = " where (rownumber>=" + Parking_rowOffset + ") and (rownumber<" + (Parking_rowOffset + Parking_rowLimit).ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                string sql = @"SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Tbl_ParkingEnterAndExit.ID desc ) AS rownumber,Tbl_Parking.ID, InttcardDriver ,Name,Family,Tbl_ParkingEnterAndExit.ID as IDEnterAndExit,Tbl_ParkingEnterAndExit.IsCancel,
                                Mellicard ,Pelak,Pelakseries,InttcardVehicle
                                ,Model,vehicleTypes,dbo.MiladiToShamsi(DateEnter)as DateEnter,dbo.MiladiToShamsi(DateExit)as DateExit,DateEnter as MiladiDateEnter,DateExit as MiladiDateExit,Tbl_Parking.GroupID,GroupName,IsInterim
                                ,UserEnterInsert,UserExitInsert
                                ,userEnter, 
                                userExit
                                ,Tbl_ParkingEnterAndExit.CostParking
                                ,substring(replace(Cast(cast(DateEnter as Time)as nvarchar(50)),':',''),0,5) as  TimeEnter
                                ,substring(replace(Cast(cast(DateExit as Time)as nvarchar(50)),':',''),0,5) as  TimeExit
                                ,UserCancel from Tbl_Parking INNER JOIN Tbl_ParkingEnterAndExit ON Tbl_Parking.ID = Tbl_ParkingEnterAndExit.ParkingID  " + filter_str1 + ") as Tbl_Parking " + filter_str2;
                dgParking.DataSource = dal.select(sql);
                dal.disConnect();
                //this.tbl_DriversTableAdapter.Adapter.SelectCommand.CommandText = "SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Id) AS rownumber,* from Tbl_Parking " + filter_str1 + ") as Tbl_Parking " + filter_str2;
                //this.tbl_DriversTableAdapter.Fill(this.sSDB_Drivers.Tbl_Drivers);
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: getParkingFilter()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات رانندگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledParkingBtn();
            }
        }
        public void checkEnabledParkingBtn()
        {
            try
            {
                btnShowNextParking.Visible = true;
                btnShowFirstParking.Visible = true;
                btnShowLastParking.Visible = true;
                btnShowPriviousParking.Visible = true;
                if (Parking_rowOffset <= 1)
                {

                    btnShowFirstParking.Visible = false;
                    btnShowPriviousParking.Visible = false;
                }
                if ((Parking_rowOffset + Parking_rowLimit) > Parking_rowCount)
                {
                    btnShowNextParking.Visible = false;
                    btnShowLastParking.Visible = false;
                }
                this.lblFromRecord_Parking.Text = Parking_rowOffset.ToString();
                Int32 tmp = Parking_rowOffset + this.dgParking.RowCount - 1;
                if (tmp < 0)
                    tmp = 0;
                this.lbltoRecord_Parking.Text = (tmp).ToString();
                this.lblAllRecord_Parking.Text = Parking_rowCount.ToString();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: getParkingFilter()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("در نمایش اطلاعات خطا بوجود آمد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void btnShowLastParking_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_str1 = Parking_Selectquery;
                Parking_rowOffset = Parking_rowCount - Parking_rowLimit + 1;
                if (Parking_rowOffset <= 0)
                    Parking_rowOffset = 1;
                Int32 tmp = Parking_rowOffset + Parking_rowLimit;
                string filter_str2 = " where (rownumber>=" + Parking_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                string sql = @"SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Tbl_ParkingEnterAndExit.ID desc) AS rownumber,Tbl_Parking.ID, InttcardDriver ,Name,Family,Tbl_ParkingEnterAndExit.ID as IDEnterAndExit,Tbl_ParkingEnterAndExit.IsCancel,
Mellicard ,Pelak,Pelakseries,InttcardVehicle
,Model,vehicleTypes,dbo.MiladiToShamsi(DateEnter)as DateEnter,dbo.MiladiToShamsi(DateExit)as DateExit,DateEnter as MiladiDateEnter,DateExit as MiladiDateExit,Tbl_Parking.GroupID,GroupName,IsInterim
,UserEnterInsert,UserExitInsert
,userEnter, 
userExit
,Tbl_ParkingEnterAndExit.CostParking
,substring(replace(Cast(cast(DateEnter as Time)as nvarchar(50)),':',''),0,5) as  TimeEnter
,substring(replace(Cast(cast(DateExit as Time)as nvarchar(50)),':',''),0,5) as  TimeExit
,UserCancel from Tbl_Parking INNER JOIN Tbl_ParkingEnterAndExit ON Tbl_Parking.ID = Tbl_ParkingEnterAndExit.ParkingID  " + filter_str1 + ") as Tbl_Parking " + filter_str2;
                dgParking.DataSource = dal.select(sql);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowLastParking_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات ناوگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledParkingBtn();
            }
        }

        private void btnShowNextParking_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_str1 = Parking_Selectquery;
                Parking_rowOffset = Parking_rowOffset + Parking_rowLimit;
                Int32 tmp = Parking_rowOffset + Parking_rowLimit;
                if (tmp > Parking_rowCount)
                    Parking_rowOffset = Parking_rowCount - Parking_rowLimit + 1;

                string filter_str2 = " where (rownumber>=" + Parking_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                string sql = @"SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Tbl_ParkingEnterAndExit.ID desc) AS rownumber,Tbl_Parking.ID, InttcardDriver ,Name,Family,Tbl_ParkingEnterAndExit.ID as IDEnterAndExit,Tbl_ParkingEnterAndExit.IsCancel,
Mellicard ,Pelak,Pelakseries,InttcardVehicle
,Model,vehicleTypes,dbo.MiladiToShamsi(DateEnter)as DateEnter,dbo.MiladiToShamsi(DateExit)as DateExit,DateEnter as MiladiDateEnter,DateExit as MiladiDateExit,Tbl_Parking.GroupID,GroupName,IsInterim
,UserEnterInsert,UserExitInsert
,userEnter, 
userExit
,Tbl_ParkingEnterAndExit.CostParking
,substring(replace(Cast(cast(DateEnter as Time)as nvarchar(50)),':',''),0,5) as  TimeEnter
,substring(replace(Cast(cast(DateExit as Time)as nvarchar(50)),':',''),0,5) as  TimeExit
,UserCancel from Tbl_Parking INNER JOIN Tbl_ParkingEnterAndExit ON Tbl_Parking.ID = Tbl_ParkingEnterAndExit.ParkingID  " + filter_str1 + ") as Tbl_Parking " + filter_str2;
                dgParking.DataSource = dal.select(sql);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowNextParking_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات ناوگان وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledParkingBtn();
            }
        }

        private void btnShowPriviousParking_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_str1 = Parking_Selectquery;
                Parking_rowOffset = Parking_rowOffset - Parking_rowLimit;
                if (Parking_rowOffset < 1)
                    Parking_rowOffset = 1;
                Int32 tmp = Parking_rowOffset + Parking_rowLimit;
                string filter_str2 = " where (rownumber>=" + Parking_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                string sql = @"SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Tbl_ParkingEnterAndExit.ID desc) AS rownumber,Tbl_Parking.ID, InttcardDriver ,Name,Family,Tbl_ParkingEnterAndExit.ID as IDEnterAndExit,Tbl_ParkingEnterAndExit.IsCancel,
Mellicard ,Pelak,Pelakseries,InttcardVehicle
,Model,vehicleTypes,dbo.MiladiToShamsi(DateEnter)as DateEnter,dbo.MiladiToShamsi(DateExit)as DateExit,DateEnter as MiladiDateEnter,DateExit as MiladiDateExit,Tbl_Parking.GroupID,GroupName,IsInterim
,UserEnterInsert,UserExitInsert
,userEnter, 
userExit
,Tbl_ParkingEnterAndExit.CostParking
,substring(replace(Cast(cast(DateEnter as Time)as nvarchar(50)),':',''),0,5) as  TimeEnter
,substring(replace(Cast(cast(DateExit as Time)as nvarchar(50)),':',''),0,5) as  TimeExit
,UserCancel from Tbl_Parking INNER JOIN Tbl_ParkingEnterAndExit ON Tbl_Parking.ID = Tbl_ParkingEnterAndExit.ParkingID  " + filter_str1 + ") as Tbl_Parking " + filter_str2;
                dgParking.DataSource = dal.select(sql);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowPriviousParking_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات پارکینگ وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledParkingBtn();
            }
        }

        private void btnShowFirstParking_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_str1 = Parking_Selectquery;
                Parking_rowOffset = 1;
                Int32 tmp = Parking_rowLimit + 1;
                string filter_str2 = " where (rownumber>=" + Parking_rowOffset + ") and (rownumber<" + tmp.ToString() + ") ";
                DataAccessLayerSepehr dal = new DataAccessLayerSepehr();
                dal.connect();
                string sql = @"SELECT * FROM (select ROW_NUMBER() OVER (ORDER BY Tbl_ParkingEnterAndExit.ID desc) AS rownumber,Tbl_Parking.ID, InttcardDriver ,Name,Family,Tbl_ParkingEnterAndExit.ID as IDEnterAndExit,Tbl_ParkingEnterAndExit.IsCancel,
Mellicard ,Pelak,Pelakseries,InttcardVehicle
,Model,vehicleTypes,dbo.MiladiToShamsi(DateEnter)as DateEnter,dbo.MiladiToShamsi(DateExit)as DateExit,DateEnter as MiladiDateEnter,DateExit as MiladiDateExit,Tbl_Parking.GroupID,GroupName,IsInterim
,UserEnterInsert,UserExitInsert
,userEnter, 
userExit
,Tbl_ParkingEnterAndExit.CostParking
,substring(replace(Cast(cast(DateEnter as Time)as nvarchar(50)),':',''),0,5) as  TimeEnter
,substring(replace(Cast(cast(DateExit as Time)as nvarchar(50)),':',''),0,5) as  TimeExit
,UserCancel from Tbl_Parking INNER JOIN Tbl_ParkingEnterAndExit ON Tbl_Parking.ID = Tbl_ParkingEnterAndExit.ParkingID  " + filter_str1 + ") as Tbl_Parking " + filter_str2;
                dgParking.DataSource = dal.select(sql);
                dal.disConnect();
            }
            catch (Exception ex)
            {
                Helper.CsHelper.writeLog("FrmMain :: btnShowFirstParking_Click()> " + ex.Message.ToString());
                Helper.CsHelper.showMessage("امکان نمایش اطلاعات پارکینگ وجود ندارد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                checkEnabledParkingBtn();
            }
        }

        private void btnAlmasInsert_Click(object sender, EventArgs e)
        {
            new Forms.FrmAlmasInsert().ShowDialog();
        }

        private void btnAlmasRefresh_Click(object sender, EventArgs e)
        {
            DGV_Almas.AutoGenerateColumns = false;
            DGV_Almas.DataSource = DAL.CsDALAlmasInnings.Select("");
        }

        private void tabAlmasInnings_Enter(object sender, EventArgs e)
        {
            btnAlmasRefresh_Click(sender, e);
        }

        private void txtN_EnterDate_TextChanged_1(object sender, EventArgs e)
        {
            btnSetParkingFilter_Click(sender, e);
        }

        private void txtVehicleTypes_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            btnSetParkingFilter_Click(sender, e);

        }
//        public string QueryInnings = @"
//select * From (SELECT ROW_NUMBER() OVER (ORDER BY Tbl_Innings.InningsID desc) AS rownumber,
//    dbo.Tbl_Innings.InningsID,Tbl_Innings.VehileID,Tbl_Innings.DriverID, dbo.Tbl_Innings.InttcardDriver, dbo.Tbl_Innings.Name, dbo.Tbl_Innings.Family, dbo.Tbl_Innings.Mellicard, 
//                      dbo.Tbl_Innings.InttcardExpDate, Pelak, Pelakseries,Tbl_Innings.InttcardVehicle, Tbl_Innings.Model, Tbl_Innings.vehicleTypes, dbo.Tbl_InningsStatue.Name AS StatusName, 
//                      dbo.MiladiToShamsi(dbo.Tbl_Innings.SaveDate) AS SaveDateShamsi, dbo.Tbl_Innings.SaveDate,dbo.Tbl_Innings.StatueID,dbo.Tbl_Innings.InningsGroupID, dbo.Tbl_InningsGroup.Name AS GroupName
//                      ,Tbl_Innings.UserSave,Tbl_user.Name+' '+Tbl_user.LastName as UserName
//                    ,substring(replace(Cast(cast(SaveDate as Time)as nvarchar(50)),':',''),0,5) as  SaveTime,Tbl_Innings.UserSave,case when StatueID<>7 then  Tbl_Innings.Cost else 0 end Cost --StatueID=7 =>انصراف -عودت پول هزینه صفر می شود
//FROM         dbo.Tbl_Innings INNER JOIN
//                      dbo.Tbl_InningsStatue ON dbo.Tbl_Innings.StatueID = dbo.Tbl_InningsStatue.ID 
//					  " + filter_str1 + ") as Tbl_Innings " + filter_str2;
    }
}
