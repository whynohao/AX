webpackJsonp([12],{

/***/ 228:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "EnumNavHeader", function() { return EnumNavHeader; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "NavHeaderItems", function() { return NavHeaderItems; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "NavHeaderItemUtility", function() { return NavHeaderItemUtility; });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_babel_runtime_core_js_set__ = __webpack_require__(154);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_babel_runtime_core_js_set___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_0_babel_runtime_core_js_set__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_babel_runtime_core_js_array_from__ = __webpack_require__(152);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_babel_runtime_core_js_array_from___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_1_babel_runtime_core_js_array_from__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_babel_runtime_helpers_createClass__ = __webpack_require__(26);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_babel_runtime_helpers_createClass___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_2_babel_runtime_helpers_createClass__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3_babel_runtime_helpers_classCallCheck__ = __webpack_require__(19);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3_babel_runtime_helpers_classCallCheck___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_3_babel_runtime_helpers_classCallCheck__);





var EnumNavHeader = function EnumNavHeader() {
  __WEBPACK_IMPORTED_MODULE_3_babel_runtime_helpers_classCallCheck___default()(this, EnumNavHeader);
};

EnumNavHeader.workbench = 'workbench';
EnumNavHeader.business = 'business';
EnumNavHeader.report = 'report';
EnumNavHeader.kpi = 'kpi';
EnumNavHeader.board = 'board';
EnumNavHeader.document = 'document';
EnumNavHeader.menuConfig = 'menuConfig';
EnumNavHeader.cpsModel = 'cpsModel';

var defaultHeaderItem = {
  key: EnumNavHeader.workbench,
  text: '工作台',
  contrainer: 'workbench',
  enabled: true
};
var NavHeaderItems = [defaultHeaderItem, {
  key: EnumNavHeader.business,
  text: '业务功能',
  contrainer: EnumNavHeader.business,
  enabled: true
}, {
  key: EnumNavHeader.report,
  text: '报表管理',
  contrainer: EnumNavHeader.business,
  enabled: true
}, {
  key: EnumNavHeader.kpi,
  text: 'KPI管理',
  contrainer: EnumNavHeader.business,
  enabled: true
}, {
  key: EnumNavHeader.board,
  text: '看板管理',
  contrainer: EnumNavHeader.business,
  enabled: false
}, {
  key: EnumNavHeader.document,
  text: '文档管理',
  contrainer: EnumNavHeader.document,
  enabled: true
}, {
  key: EnumNavHeader.cpsModel,
  text: 'CPS建模',
  contrainer: 'cpsModel',
  enabled: true
}, {
  key: EnumNavHeader.menuConfig,
  text: '功能菜单',
  contrainer: EnumNavHeader.menuConfig,
  enabled: true
}];

var NavHeaderItemUtility = function () {
  function NavHeaderItemUtility() {
    __WEBPACK_IMPORTED_MODULE_3_babel_runtime_helpers_classCallCheck___default()(this, NavHeaderItemUtility);
  }

  __WEBPACK_IMPORTED_MODULE_2_babel_runtime_helpers_createClass___default()(NavHeaderItemUtility, null, [{
    key: 'getDefaultHeaderItem',
    value: function getDefaultHeaderItem() {
      return defaultHeaderItem;
    }
  }, {
    key: 'getContrainers',
    value: function getContrainers() {
      return __WEBPACK_IMPORTED_MODULE_1_babel_runtime_core_js_array_from___default()(new __WEBPACK_IMPORTED_MODULE_0_babel_runtime_core_js_set___default.a(NavHeaderItems.map(function (item) {
        return item.contrainer;
      })));
    }
  }]);

  return NavHeaderItemUtility;
}();



/***/ })

},[228]);
//# sourceMappingURL=nav-header-items.021cb716e5084c7d072a.js.map