﻿using Epicor.Mfg.BO;
using Epicor.Mfg.Core;
using Epicor.Mfg.Lib;
using System;
using System.Data;

namespace JobStatusByPlanner
{
    public class DataList
    {
        public static BLConnectionPool EpicConn = new BLConnectionPool(Properties.Settings.Default.uname, Properties.Settings.Default.passw, "AppServerDC://" + Properties.Settings.Default.svrname + ":" + Properties.Settings.Default.svrport);

        public static void EpicClose()
        {
                SessionMod SM = new SessionMod(EpicConn);
                
                SM.GracefulShutdown();

                Progress.Open4GL.DynamicAPI.Session Session = EpicConn.Get();

                EpicConn.Release(Session);
        }

        public static BOReader BOReader
        {
            get
            {
                BOReader BOReader = new BOReader(EpicConn);

                return BOReader;
            }

        }

        public static DataSet PlannerList()
        {
            DataSet ds = (DataSet)BOReader.GetList("Person", "", "PersonID,Name");

            EpicClose();

            return ds;
        }

        public static DataSet PlantDataSet()
        {
            DataSet ds = (DataSet)BOReader.GetList("Plant", "", "Company,Plant,Name");

            EpicClose();

            return ds;
        }

        public static DataSet PartClassDataSet()
        {
            DataSet ds = (DataSet)BOReader.GetList("PartClass", "", "ClassID,Description");

            EpicClose();

            return ds;
        }

        public static bool PartExists(string partnumber)
        {
            Part Part = new Part(EpicConn);

            PartDataSet Pdata = new PartDataSet();

            try
            {
                Pdata = Part.GetByID(partnumber);
            }
            catch
            {
                EpicClose();

                return false;
            }

            EpicClose();

            return true;
        }

        public static string AdvanceRevision(string CurrentRevision)
        {
            char[] InVal = CurrentRevision.ToUpper().ToCharArray();

            long retval = 0;

            for (int i = InVal.GetUpperBound(0); i >= 0; i--)
            {
                int charval = (int)InVal[i];

                int x = InVal.GetUpperBound(0);

                retval += (int)Math.Pow(26, x - i) * ((int)InVal[i] - 64);

            }

            retval++;

            string s = "";
            for (long i = (long)Convert.ToDouble(Math.Log(Convert.ToDouble(25 * (Convert.ToDouble(retval) + 1))) / Math.Log(26)) - 1; i >= 0; i--)
            {
                long x = (long)Convert.ToDouble(Math.Pow(26, i + 1) - 1) / 25 - 1;
                if (retval > x)
                {
                    s += (char)(((retval - x - 1) / Convert.ToDouble(Math.Pow(26, i))) % 26 + 65);
                }
            }
            return s;
        }

        public static DataSet EngWB_DS(string GroupID, string PartNumber, string Rev)
        {
            EngWorkBench EngWB = new EngWorkBench(EpicConn);

            EngWorkBenchDataSet EngWBDS = new EngWorkBenchDataSet();

            EngWBDS = EngWB.GetDatasetForTree(GroupID,PartNumber,Rev, "", DateTime.Today, false, false);

            return (DataSet)EngWBDS;
        }

        /// <summary>
        /// Adds data in specified column at row number and table all into PartDataSet given
        /// </summary>
        /// <param name="Part"></param>
        /// <param name="tableName"></param>
        /// <param name="rowNum"></param>
        /// <param name="colName"></param>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static PartDataSet AddDatum(PartDataSet Part, string tableName, int rowNum, string colName, string Input, DataViewRowState RowState)
        {
            DataTable PartDT = Part.Tables[tableName];

            DataRow[] WorkRow = PartDT.Select(null, null, RowState);

            WorkRow[0] = PartDT.Rows[rowNum];

            try
            {
                WorkRow[0][colName] = Input;
            }
            catch (System.Exception ex)
            {
                try
                {
                    WorkRow[0][colName] = double.Parse(Input);
                }
                catch //(System.Exception ex1)
                {
                    try
                    {
                        WorkRow[0][colName] = (int)(double.Parse(Input));
                    }
                    catch //(System.Exception ex2)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.Message, "Error!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                    }
                }

            }

            return Part;
        }

        public static PartDataSet UpdateDatum(PartDataSet Part, string tableName, int rowNum, string colName, string Input)
        {
            try
            {
                Part.Tables[tableName].Rows[rowNum][colName] = Input;

                return Part;
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                return Part;	
            }
        }

        public static DataSet ProdGrupDataSet()
        {
            DataSet ds = (DataSet)BOReader.GetList("ProdGrup", "", "ProdCode,Description");

            EpicClose();

            return ds;
        }

        public static DataSet UOMSearchDataSet()
        {
            DataSet ds = (DataSet)BOReader.GetList("UOMSearch", "((Active=True) AND (UOMClassID = 'NORCO'))", "UOMCode,UOMDesc");

            EpicClose();

            return ds;
        }

        public static DataSet UOMClassDataSet()
        {
            BOReader BOReader = new BOReader(EpicConn);

            DataSet ds = (DataSet)BOReader.GetList("UOMClass", "((Active=True) AND (ClassType<>'OnTheFly'))", "UOMClassID,Description");

            EpicClose();

            return ds;
        }

        public static DataSet UOMWeightDataSet()
        {
            DataSet ds = (DataSet)BOReader.GetList("UOMSearch", "((Active=True) AND (ClassType='Weight'))", "UOMCode,UOMDesc");

            EpicClose();

            return ds;
        }

        public static DataSet UOMVolumeDataSet()
        {
            DataSet ds = (DataSet)BOReader.GetList("UOMSearch", "((Active=True) AND (ClassType='Volume'))", "UOMCode,UOMDesc");

            EpicClose();

            return ds;
        }

        public static DataSet WarehseDataSet()
        {
            //Should diversify this call to use other than MfgSys as plant
            DataSet ds = (DataSet)BOReader.GetList("WarehseSearch", "MfgSys", "");

            EpicClose();

            return ds;
        }

        public static DataSet GroupIDDataSet()
        {
            bool MorePages;

            EngWorkBench EngBench = new EngWorkBench(EpicConn);

            DataSet ds = (DataSet)EngBench.GetList(" BY GroupID", 100, 0, out MorePages);

            EpicClose();

            return ds;
        }

        public static DataSet ResourceGroup()
        {
            DataSet ds = (DataSet)BOReader.GetList("ResourceGroup", "Inactive=False", "ResourceGrpID,Inactive,Description");

            EpicClose();

            return ds;
        }

        public static DataSet Resource(string ResourceGrpId)
        {
            DataSet ds = (DataSet)BOReader.GetList("Resource", "(Inactive=False) AND ResourceGrpID='" + ResourceGrpId +"'", "ResourceID,Inactive,ResourceGrpId,Description");

            EpicClose();

            return ds;
        }

        public static void CheckOutPart(string GroupID, string PartNumber, string Revision)
        {
            string CheckedOutRevNum;

            string altMethodMsg;

            bool altMethodFlg;

            EngWorkBench EngWb = new EngWorkBench(EpicConn);

            EngWb.CheckOut(GroupID, PartNumber, Revision, "", DateTime.Today, false, false, false, true, false, out CheckedOutRevNum, out altMethodMsg, out altMethodFlg);

            EpicClose();
        }

        public static void UndoCheckOutPart(string GroupID, string PartNumber, string Revision)
        {
            EngWorkBench EngWb = new EngWorkBench(EpicConn);

            EngWorkBenchDataSet ds = new EngWorkBenchDataSet();

            ds = EngWb.GetDatasetForTree(GroupID,PartNumber,Revision,"",DateTime.Today,false,true);

            EngWb.UndoCheckOut(GroupID, PartNumber, Revision,"", DateTime.Today, false, false, false, true, ds);
        }

        public static void ApprovePart(EngWorkBenchDataSet EngDataSet,string GroupID,string PartNumber, string Revision)
        {
            EngWorkBench EngWb = new EngWorkBench(EpicConn);

            EngDataSet = EngWb.GetDatasetForTree(GroupID, PartNumber, Revision, "", DateTime.Now, false, false);

            EngDataSet.Tables["ECORev"].Rows[0]["Approved"] = true;

            EngWb.CheckECORevApproved(true, false, EngDataSet);

            EngWb.Update(EngDataSet);

            EpicClose();
        }

        public static void CheckInPart(string GroupID, string PartNumber, string Revision)
        {
            EngWorkBench EngWb = new EngWorkBench(EpicConn);

            string opMessage;

            EngWb.CheckIn(GroupID, PartNumber, Revision, "", DateTime.Now, false, false, true, true, false, "FOR EPICOR INTEGRATION MODULE", out opMessage);

            EpicClose();
        }

        /// <summary>
        /// Search Function for retrieving Part lists
        /// </summary>
        /// <param name="WhereStatement">Equivalent to the SQL WHERE function; Leave blank for all possiblities</param>
        /// <returns>Dataset of parts meeting the WhereStatement criteria</returns>
        public PartDataSet PartSearchDataSet(string WhereStatement)
        {
            Part Part = new Part(EpicConn);

            bool More;

            DataSet dss = ((DataSet)Part.GetList(WhereStatement, 0, 0, out More));

            PartDataSet ds = (PartDataSet)dss;

            EpicClose();

            return ds;
        }

        public DataSet GetProjectRoles()
        {
            EngWorkBench EngWB = new EngWorkBench(EpicConn);

            DataSet MethodReturned = EngWB.GetProjectRoles();

            EpicClose();

            return MethodReturned;
        }

        public string GetCodeDescList()
        {
            EngWorkBench EngWB = new EngWorkBench(EpicConn);

            string ReturnMethod = EngWB.GetCodeDescList("ECORev", "ProcessMode");

            EpicClose();

            return ReturnMethod;
        }

        public void GetAvailTaskSets(out string NewList)
        {
            EngWorkBench EngWB = new EngWorkBench(EpicConn);

            EngWB.GetAvailTaskSets(out NewList);

            EpicClose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipGroupID">ECO Group ID</param>
        public void GroupLock(string ipGroupID)
        {
            EngWorkBench EngWB = new EngWorkBench(EpicConn);

            EngWB.GroupLock(ipGroupID);

            EpicClose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipGroupID">ECO Group ID</param>
        /// <param name="ipCheckOutStatus">Normally should be true</param>
        /// <returns></returns>
        public EngWorkBenchDataSet GetECORevData(string ipGroupID, bool ipCheckOutStatus)
        {
            EngWorkBench EngWB = new EngWorkBench(EpicConn);

            EngWorkBenchDataSet ReturnMethod = EngWB.GetECORevData(ipGroupID, ipCheckOutStatus);

            EpicClose();

            return ReturnMethod;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipGroupID">ECO Group ID</param>
        /// <param name="ipPartNum">Part Number</param>
        /// <param name="ipRevisionNum">Revision Number</param>
        /// <param name="ipAltMethod">Normally Null string</param>
        /// <param name="ipAsOfDate">Current Date/Time</param>
        /// <param name="ipCompleteTree">Normally False</param>
        /// <param name="ipUseMEthodForParts">Normally True</param>
        /// <returns></returns>
        public EngWorkBenchDataSet GetDatasetForTree(string ipGroupID, string ipPartNum, string ipRevisionNum, string ipAltMethod, DateTime? ipAsOfDate, bool ipCompleteTree, bool ipUseMEthodForParts)
        {
            EngWorkBench EngWB = new EngWorkBench(EpicConn);

            EngWorkBenchDataSet MethodReturned = EngWB.GetDatasetForTree(ipGroupID,ipPartNum,ipRevisionNum,ipAltMethod,ipAsOfDate,ipCompleteTree,ipUseMEthodForParts);

            EpicClose();

            return MethodReturned;
        }

        public ECOGroupListDataSet GetList(string ipGroupID)
        {
            EngWorkBench EngWB = new EngWorkBench(EpicConn);

            bool morePages;

            ECOGroupListDataSet ReturnMethod = EngWB.GetList("GroupID = '" + ipGroupID + "' BY GroupID", 0, 0, out morePages);

            EpicClose();

            return ReturnMethod;
        }

        public EngWorkBenchDataSet CheckOut(string ipGroupID, string ipPartNum, string ipRevisionNum, string ipAltMethod, DateTime? ipAsOfDate, bool ipCompleteTree, bool ipValidPassword, bool ipReturn, bool ipGetDatasetForTree, bool ipUseMethodForParts, out string opCheckedOutRevisionNum, out string altMethodMsg, out bool altMethodFlg)
        {
            EngWorkBench EngWB = new EngWorkBench(EpicConn);

            EngWorkBenchDataSet ReturnMethod = EngWB.CheckOut(ipGroupID, ipPartNum, ipRevisionNum, ipAltMethod, ipAsOfDate, ipCompleteTree, ipValidPassword, ipReturn, ipGetDatasetForTree, ipUseMethodForParts, out opCheckedOutRevisionNum, out altMethodMsg, out altMethodFlg);

            EpicClose();

            return ReturnMethod;
        }

        public static string GetCurrentDesc(string PartNumber)
        {
            try
            {
                Part Part = new Part(EpicConn);

                PartDataSet PartData = new PartDataSet();

                PartData = Part.GetByID(PartNumber);

                int LastRowIndex = PartData.Tables["Part"].Rows.Count - 1;

                string PartDesc = PartData.Tables["Part"].Rows[LastRowIndex]["PartDescription"].ToString();

                EpicClose();

                return PartDesc;
            }
            catch { return null; }
        }

        public static string GetCurrentRev(string PartNumber)
        {
            try
            {
                Part Part = new Part(DataList.EpicConn);

                PartDataSet PartData = new PartDataSet();

                PartData = Part.GetByID(PartNumber);

                int LastRowIndex = PartData.Tables["PartRev"].Rows.Count - 1;

                string PartRev = PartData.Tables["PartRev"].Rows[LastRowIndex]["RevisionNum"].ToString();

                EpicClose();

                return PartRev;
            }
            catch { return ""; }
        }

        public static DataTable PartUOM(string PartNumber)
        {
            try
            {
                Part Part = new Part(EpicConn);

                PartDataSet PartData = new PartDataSet();

                PartData = Part.GetByID(PartNumber);

                EpicClose();

                return PartData.Tables["PartUOM"];
            }
            catch { return null; }
        }

        public static bool CreatePartRevision(string PartNumber, string CurrentRev, string NewRev, string RevDesc)
        {
            bool _results;

            try
            {
                Part Part = new Part(DataList.EpicConn);

                PartDataSet PartData = new PartDataSet();

                PartData = Part.GetByID(PartNumber);

                Part.GetNewPartRev(PartData, PartNumber, CurrentRev);

                int Y = PartData.Tables["PartRev"].Rows.Count - 1;

                DataList.UpdateDatum(PartData, "PartRev", Y, "RevShortDesc", RevDesc);

                DataList.UpdateDatum(PartData, "PartRev", Y, "RevisionNum", NewRev);

                DataList.UpdateDatum(PartData, "PartRev", Y, "AltMethod", "");

                Part.Update(PartData);

                _results = true;
            }
            catch { _results = false; }
            finally
            {
                EpicClose();
            }

            return _results;
        }
    }

    public class ECOGroup
    {
        public string Company;
        public string GroupID;
        public string Description;
        public bool GroupClosed;
        public string Character01;
        public string Character02;
        public string Character03;
        public string Character04;
        public string Character05;
        public string Character06;
        public string Character07;
        public string Character08;
        public string Character09;
        public string Character10;
        public int Number01;
        public int Number02;
        public int Number03;
        public int Number04;
        public int Number05;
        public int Number06;
        public int Number07;
        public int Number08;
        public int Number09;
        public int Number10;
        public bool CheckBox01;
        public bool CheckBox02;
        public bool CheckBox03;
        public bool CheckBox04;
        public bool Checkbox05;
        public string CommentText;
        public DateTime DueDate;
        public DateTime CreatedDate;
        public string CreatedBy;
        public int CreatedTime;
        public string ClosedBy;
        public int ClosedTime;
        public string ECO;
        public string TaskSetID;
        public string CurrentWFStageID;
        public string ActiveTaskID;
        public string LastTaskID;
        public bool CheckInAllowed;
        public string PrimeSalesRepCode;
        public string WFGroupID;
        public bool CheckOutAllowed;
        public bool WFComplete;
        public bool SingleUser;
        public bool GrpLocked;
        public string GrpLockedBy;
        public int Number11;
        public int Number12;
        public int Number13;
        public int Number14;
        public int Number15;
        public int Number16;
        public int Number17;
        public int Number18;
        public int Number19;
        public int Number20;
        public bool CheckBox06;
        public bool CheckBox07;
        public bool CheckBox08;
        public bool CheckBox09;
        public bool CheckBox10;
        public bool CheckBox11;
        public bool CheckBox12;
        public bool CheckBox13;
        public bool CheckBox14;
        public bool CheckBox15;
        public bool CheckBox16;
        public bool CheckBox17;
        public bool CheckBox18;
        public bool CheckBox19;
        public bool CheckBox20;
        public string ShortChar01;
        public string ShortChar02;
        public string ShortChar03;
        public string ShortChar04;
        public string ShortChar05;
        public string ShortChar06;
        public string ShortChar07;
        public string ShortChar08;
        public string ShortChar09;
        public string ShortChar10;
        public Guid SysRowID;
        public string SysRevID;
        public byte BitFlag;
        public bool MassAssignDesc;
        public bool MassAssignECO;
        public bool MassAssignEffectiveDate;
        public bool CanApproveAll;
        public bool MultiBOMAllowed;
        public bool CanCheckInAll;
        public bool WFGroupIDDesc;
        public bool UseMethodForPartsInTree;
        public string CurrentWFStageDesc;
        public bool EnableCheckInAll;
        public string TaskSetIDTastSetDescription;
        public string TaskSetIDWorkflowType;
    }

    /// <summary>
    /// 
    /// </summary>
    class BOMData
    {

    }

    /// <summary>
    /// 
    /// </summary>
    class BOOData
    {

    }

    /// <summary>
    /// 
    /// </summary>
    class Operation
    {

    }

    /// <summary>
    /// 
    /// </summary>
    class PartTypeCode
    {
        private string _Code;
        private string _Description;

        public PartTypeCode(string Description, string Code)
        {
            _Code = Code;
            _Description = Description;
        }

        /// <summary>
        /// Overridden to prove correct data member for combobox
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _Code;
        }

        public string Code
        {
            get
            {
                return _Code;
            }
        }

        public string Description
        {
            get
            {
                return _Description;
            }
        }
    }

    public class ProdStdType
    {
        private string _Code;
        private string _Desc;

        public ProdStdType(string Description, string Code)
        {
            _Code = Code;
            _Desc = Description;
        }

        public override string ToString()
        {
            return _Code;
        }

        public string Code
        {
            get
            {
                return _Code;
            }
        }

        public string Description
        {
            get
            {
                return _Desc;
            }
        }
    }

    public class MtlReseqType
    {
        private string _Name;
        private string _Arg;

        public MtlReseqType(string Name, string Arg)
        {
            _Name = Name;

            _Arg = Arg;
        }

        public override string ToString()
        {
            return _Arg;
        }

        public string Arg
        {
            get
            {
                return _Arg;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }
    }

     /// <summary>
    /// Data structure for Part with all appropriate descriptors
    /// </summary>
    public class PartData
    {
        public string PartNumber;

        public string Description;

        public string PMT;

        public string UOM_Class;

        public decimal Net_Weight;

        public decimal Net_Vol;

        public string Net_Weight_UM;

        public string Net_Vol_UM;

        public string Primary_UOM;

        public string PartGroup;

        public string PartClass;

        public string PartPlant;

        public string PlantWhse;

        public bool QtyBearing;

        public bool UseRevision;

        public bool TrackSerial;
    }


}
