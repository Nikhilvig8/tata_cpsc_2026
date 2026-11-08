# Checkmarx remediation (scan reports 2026-09-10: TASS-CV / CPSC-CV)

The same changes were applied to both repositories (CPSC and TASS).

## Deployment steps (required)
1. **Secrets are no longer in source control.** On every server and developer machine, create these files in `CPSC(Tata)/` (both are git-ignored):
   - `connectionStrings.config`: copy `connectionStrings.config.example` and fill in the real connection strings.
   - `secrets.config`: copy `secrets.config.example` (SMTP, Tableau connected app, and on CPSC the SendGrid API key).
   The site will not start without `connectionStrings.config`.
2. **Rotate every credential that was previously committed.** It is still in git history: DB passwords (`sa`, `APPDBADMIN`), the SMTP password, the Tableau connected-app secret and the SendGrid API key.
3. Optional appSetting `CanonicalHost` (TASS, in Global.asax): the host name used for the HTTP→HTTPS redirect.
4. Run the Checkmarx scan on a clean `git archive`/GitHub zip. A zip of a working folder would include the secret files and the untracked `packages/` folder.

## SAST
| Finding | Fix |
|---|---|
| SQL_Injection | Stored-procedure names taken from the request are allow-listed (`App_Code/SqlGuard.cs`). Concatenated SQL in `ActionPlanController`, `Transaction.cs` and `DropDownController.Tempdataremove` is now parameterized. Dead dynamic-SQL helpers were removed from `DataUtility.cs`. |
| CSRF | Global `AntiForgeryOnPostAttribute`, plus `[ValidateAntiForgeryToken]` on every POST action. `Scripts/csrf-guard.js` adds the token to all forms and AJAX POSTs. State-changing GET actions (ActionPlan, Notification, BULKNotification) are now split into GET (view) and POST (submit). |
| Reflected_XSS | Uploaded column names are HTML-encoded. The HTTP→HTTPS redirect is built from encoded parts. The unused `Base64Decode` action was removed. |
| Stored/relative Path Traversal | Report export/download file names go through `SqlGuard.SafeReportFileName`. The upload category name is validated. |
| Dangerous_File_Extension | `App_Code/UploadGuard.cs` checks extension, file signature and size, and saves under a server-generated name (Excel uploads go to `App_Data/Uploads`, deleted after processing). |
| XXE | The dead SOAP `IsValidOID` login (string-built XML + `LoadXml`) was removed. |
| Hardcoded passwords / connection strings / password in comment | Moved to config. Backup files were deleted (`*_old`, `*_030223`, `LoginController_251023/261023`, etc.); two of them contained login backdoors. |
| HttpOnly_Cookie_Flag_Not_Set | All cookies set `HttpOnly` and `Secure`. |
| Missing_HSTS_Header | `max-age=31536000; includeSubDomains`, from Web.config and code (including the redirect). CPSC: the unused OWIN/Identity template, where the finding was, was removed. |
| Privacy_Violation | The Keycloak error body is no longer written to the console. |

## SCA
- Removed 85 Metronic theme plugins that no page loads: AngularJS, CKEditor, CodeMirror, Dropzone, Plupload, Summernote, bootstrap-select/table/tagsinput, blueimp upload, fancybox, owl-carousel, old jQuery copies and others. Also removed the demo page scripts, the plugin demos, the amCharts export plugin (JSZip 2.5), `Scripts/jszip.min.js`, the unused `pdf.js` 1.6 and bootstrap-markdown.
- Upgraded: jQuery 3.7.1 everywhere, jQuery Validation 1.22.1, Select2 4.1.0-rc.0, jQuery UI 1.13.3, moment 2.30.1, DataTables 1.13.11 (CDN copies replaced with local files).
- **Bootstrap 3 → Bootstrap 5.3.8.** `assets/global/css/bs3-compat.css` and `assets/global/scripts/bs3-compat.js` keep the legacy Bootstrap 3 markup and the Metronic theme working: `data-*` → `data-bs-*`, `col-xs`/offsets, panels, glyphicons mapped to Font Awesome, `li.active` tabs, `tooltip('destroy')`.
- NuGet: removed jQuery.Validation, Microsoft.jQuery.Unobtrusive.Validation and bootstrap (content-only; files stay in `Scripts/`). TASS: Microsoft.IdentityModel / System.IdentityModel.Tokens.Jwt 7.0.3 → 7.7.1. CPSC: Microsoft.Owin 3.0.1 and AspNet.Identity references removed, and the committed `packages/` folder was untracked.

## Must be tested before release
The UI after the Bootstrap 5 change: menus/dropdowns, modals, tabs, tooltips/popovers, forms and grids on every page. Also: bulk uploads, report Excel/PDF export, notification approval, ActionPlan insert/update, and DataFilling.
