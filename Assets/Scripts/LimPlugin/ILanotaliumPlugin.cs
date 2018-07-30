using System.Collections;
using System.Collections.Generic;
using EasyRequest;
using UnityEngine;

namespace Lanotalium.Plugin
{
    public enum Language
    {
        简体中文,
        English
    }
    public class LanotaliumContext
    {
        /// <summary>
        /// 工程是否加载
        /// </summary>
        public bool IsProjectLoaded { get; set; }
        /// <summary>
        /// 当前语言
        /// </summary>
        public Language CurrentLanguage { get; set; }
        /// <summary>
        /// 游戏逻辑管理器
        /// </summary>
        public LimTunerManager TunerManager { get; set; }
        /// <summary>
        /// 编辑器管理器
        /// </summary>
        public LimEditorManager EditorManager { get; set; }
        /// <summary>
        /// 谱面编辑管理器
        /// </summary>
        public LimOperationManager OperationManager { get; set; }
        /// <summary>
        /// 请求用户输入数据
        /// </summary>
        public EasyRequestManager UserRequest { get; set; }
        /// <summary>
        ///  显示消息
        /// </summary>
        public MessageBoxManager MessageBox { get; set; }
        /// <summary>
        /// 执行是否成功
        /// </summary>
        public bool Succeed { get; set; }
        /// <summary>
        /// 执行结果
        /// </summary>
        public string ProcessResult { get; set; }
    }
    public interface ILanotaliumPlugin
    {
        /// <summary>
        /// 本地化的插件名称
        /// </summary>
        /// <param name="language">语言种类</param>
        /// <returns>插件名称</returns>
        string Name(Language language);
        /// <summary>
        /// 本地化的插件描述
        /// </summary>
        /// <param name="language">语言种类</param>
        /// <returns>插件描述</returns>
        string Description(Language language);
        /// <summary>
        /// 执行插件功能（协程）
        /// </summary>
        /// <param name="context">Lanotalium 上下文</param>
        IEnumerator Process(LanotaliumContext context);
    }
}
