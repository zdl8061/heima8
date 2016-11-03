﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Heima8.OA.IBLL;
using Heima8.OA.Model;

namespace Heima8.OA.UI.Portal.Controllers
{
    public class ActionInfoController : BaseController
    {
        short delflagNormal = (short)Heima8.OA.Model.Enum.DelFlagEnum.Normal;
        //
        // GET: /ActionInfo/

        public IActionInfoService ActionInfoService { get; set; }
        public IUserInfoService UserInfoService { get; set; }
        public IRoleInfoService RoleInfoService { get; set; }
        public ActionResult Index()
        {
            return View();
        }

        #region 加载数据
        public ActionResult GetAllActionInfos()
        {
            //easyui: table在初始化时候自动发送以下两个参数值。
            int pageSize = int.Parse(Request["rows"] ?? "10");
            int pageIndex = int.Parse(Request["page"] ?? "1");
            int total = 0;

            var temp = ActionInfoService.GetPageEntities(pageSize, pageIndex, out total,
                                              u => u.DelFlag == delflagNormal, u => u.ID, true);

            var tempData =
                temp.Select(
                    a =>
                    new
                        {
                            a.ID,
                            a.IsMenu,
                            a.Url,
                            a.Remark,
                            a.Sort,
                            a.SubTime,
                            a.ModfiedOn,
                            a.HttpMethd,
                            a.MenuIcon,
                            a.ActionName
                        });

            var data = new { total = total, rows = tempData.ToList() };
            return Json(data, JsonRequestBehavior.AllowGet);
        } 
        #endregion

        #region Add

        public ActionResult Add(ActionInfo actionInfo)
        {
            actionInfo.ModfiedOn = DateTime.Now;
            actionInfo.SubTime = DateTime.Now;
            actionInfo.DelFlag = delflagNormal;

            actionInfo.IsMenu = Request["IsMenu"] == "true" ? true : false;

            ActionInfoService.Add(actionInfo);
            return Content("ok");
        }

        #endregion

        #region 删除
        public ActionResult Delete(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return Content("请选中要删除数据！");
            }

            //正常处理
            string[] strIds = ids.Split(',');
            List<int> idList = new List<int>();
            foreach (var strId in strIds)
            {
                idList.Add(int.Parse(strId));
            }
            //UserInfoService.DeleteList(idList);
            ActionInfoService.DeleteListByLogical(idList);
            return Content("ok");

        }
        #endregion

        #region 修改
        public ActionResult Edit(int id)
        {
            ViewData.Model = ActionInfoService.GetEntities(u => u.ID == id).FirstOrDefault();
            return View();
        }

        [HttpPost]
        public ActionResult Edit(ActionInfo userInfo)
        {
            ActionInfoService.Update(userInfo);
            return Content("ok");
        }
        #endregion

        #region 上传图片处理
        public ActionResult UploadImage()
        {
            var file= Request.Files["fileMenuIcon"];

            string path = "/UploadFiles/UploadImgs/"+Guid.NewGuid().ToString()+"-"+file.FileName;
            
            file.SaveAs(Request.MapPath(path));
            

            return Content(path);
        }
        #endregion

        #region 设置角色
        public ActionResult SetRole(int id)
        {
            //当前要设置角色的用户
            int userId = id;

            ActionInfo actionInfo = ActionInfoService.GetEntities(u => u.ID == id).FirstOrDefault();

            //把所有的角色发送 到前台
            ViewBag.AllRoles = RoleInfoService.GetEntities(u => u.DelFlag == delflagNormal).ToList();

            //用户已经关联的角色发送到前台。
            ViewBag.ExitsRoles = (from r in actionInfo.RoleInfo
                                  select r.ID).ToList();

            return View(actionInfo);

        }

        //给用户设置角色
        public ActionResult ProcessSetRole(int UId)
        {
            //第一：当前用户id  --uid
            //第二：所有打上对勾的 角色。 ---> list
            List<int> setRoleIdList = new List<int>();
            foreach (var key in Request.Form.AllKeys)
            {
                if (key.StartsWith("ckb_"))
                {
                    int roleId = int.Parse(key.Replace("ckb_", ""));
                    setRoleIdList.Add(roleId);
                }
            }

            ActionInfoService.SetRole(UId, setRoleIdList);
            return Content("ok");

        }

        #endregion
    }
}
