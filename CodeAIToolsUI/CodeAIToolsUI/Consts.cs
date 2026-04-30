namespace CodeAIToolsUI
{
    public struct Configs
    {
        public const string ADMIN = "ADMIN";
        public const string WORKER = "WORKER";
        public const string LIBS_DIR = "libs";
        public const string FLOW_FILE = "Flow.txt";
        public const string CODE_FILE = "Code.txt";
        public const string ROOT_DIR = "CodeAI_Root";
        public const string PROJECT_NAME = "CodeAITools";
        
        // public const string COD_TO_PARSE = "Java";
    }

    public struct Marks
    {
        public const string ADM_COL = "#03279e";
        public const string WORK_COL = "#03b53b";
        public const string DIR_COLOR = "#fcba03";
        public const string ADM_DRK_COL = "#00196d";
        public const string WORK_DRK_COL = "#007224";
        public const string ROOT_DIR_COLOR = "#00b3ff";
        public const string ADM_POP_TITLE_COL = "#1d4ed8";
        public const string WORK_POP_TITLE_COL = "#065f46";
        
        // public const string WORK_EDIT_COL = "#252525";
        // public const string TXT_PRIMA = "TextPrimary";
        // public const string TXT_SECA = "TextSecondary";
    }

    public struct Icons
    {
        public const string ADM_ICON = "🛡️";
        public const string FILE_ICON = "📄";
        public const string WORK_ICON = "👷";
        public const string DIR_CLOSED_ICON = "📁";
        public const string DIR_OPENED_ICON = "📂";
    }

    public struct ApiEndpoints
    {
        public const string LOG_API = "http://localhost:8080/login";
        public const string REG_API = "http://localhost:8080/register";
        public const string GET_ALL_API = "http://localhost:8080/getAll";
        public const string GET_RESP_API = "http://localhost:8080/duty/get";
        public const string TOK_LINK = "https://github.com/settings/tokens";
        public const string ACTIV_USER_API = "http://localhost:8080/getUser";
        public const string AI_EXPL_API = "http://localhost:8080/AI/explain";
        public const string JOIN_COLL_API = "http://localhost:8080/duty/save";
        public const string DEL_DUTI_API = "http://localhost:8080/duty/delete";
        public const string GET_TOK_API = "http://localhost:8080/github/token";
        public const string AI_TRANS_API = "http://localhost:8080/AI/transform";
        public const string IS_VAL_API = "http://localhost:8080/github/isValid";
        public const string CRE_GIT_API = "http://localhost:8080/github/create";
        public const string DEL_GIT_API = "http://localhost:8080/github/delete";
        public const string GET_DUTI_API = "http://localhost:8080/duty/getDuties";
        public const string DEL_PROJ_API = "http://localhost:8080/project/delete";
        public const string CRE_PROJ_API = "http://localhost:8080/project/create";
        public const string GET_ADM_API = "http://localhost:8080/teams/getAdmins";
        public const string GET_PROJS_API = "http://localhost:8080/project/getAll";
        public const string IS_GIT_EXI_API = "http://localhost:8080/github/isExist";
        public const string UPD_TOK_API = "http://localhost:8080/github/token/update";
        public const string ADD_COLL_API = "http://localhost:8080/teams/addCollaborator";
        public const string GET_COLLS_API = "http://localhost:8080/teams/getCollaborators";
        public const string REM_COLL_API = "http://localhost:8080/teams/removeCollaborator";
        
        // Subscription API endpoints
        public const string GET_SUBSCRIPTION_API = "http://localhost:8080/subscription/get";
        public const string CREATE_SUBSCRIPTION_API = "http://localhost:8080/subscription/create";
        public const string UPDATE_SUBSCRIPTION_API = "http://localhost:8080/subscription/update";
        public const string CANCEL_SUBSCRIPTION_API = "http://localhost:8080/subscription/cancel";
        public const string GET_PLANS_API = "http://localhost:8080/subscription/plans";
        
        // Local file server API endpoints
        public const string SAVE_FLOW_API = "http://localhost:8080/local/save/flow";
        public const string SAVE_CODE_API = "http://localhost:8080/local/save/code";
        public const string READ_FLOW_API = "http://localhost:8080/local/read/flow";
        public const string READ_CODE_API = "http://localhost:8080/local/read/code";
        public const string LIST_FILES_API = "http://localhost:8080/local/list";
    }
}
