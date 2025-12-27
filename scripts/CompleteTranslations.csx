#!/usr/bin/env dotnet script
/*
 * C# Script to complete missing translations in Melodee.Blazor resource files
 * Uses proper ResX file handling to preserve formatting
 * 
 * Usage: dotnet script scripts/CompleteTranslations.csx
 */

#r "nuget: System.Resources.Extensions, 8.0.0"

using System;
using System.Collections.Generic;
using System.IO;
using System.Resources.NetStandard;

// Translation dictionaries for all 100 missing keys × 5 languages
var translations = new Dictionary<string, Dictionary<string, string>>
{
    ["es-ES"] = new Dictionary<string, string>
    {
        ["Actions.Add"] = "Agregar",
        ["Actions.AddToPlaylist"] = "Agregar a lista de reproducción",
        ["Actions.AddToQueue"] = "Agregar a la cola",
        ["Actions.Apply"] = "Aplicar",
        ["Actions.Back"] = "Atrás",
        ["Actions.Clear"] = "Limpiar",
        ["Actions.Create"] = "Crear",
        ["Actions.Download"] = "Descargar",
        ["Actions.Export"] = "Exportar",
        ["Actions.Import"] = "Importar",
        ["Actions.Next"] = "Siguiente",
        ["Actions.Pause"] = "Pausar",
        ["Actions.Previous"] = "Anterior",
        ["Actions.Refresh"] = "Actualizar",
        ["Actions.Remove"] = "Eliminar",
        ["Actions.Reset"] = "Restablecer",
        ["Actions.SaveChanges"] = "Guardar cambios",
        ["Actions.Stop"] = "Detener",
        ["Actions.Submit"] = "Enviar",
        ["Actions.Update"] = "Actualizar",
        ["Admin.Charts"] = "Gráficos",
        ["Admin.Libraries"] = "Bibliotecas",
        ["Admin.Settings"] = "Configuración",
        ["Common.Album"] = "Álbum",
        ["Common.Albums"] = "Álbumes",
        ["Common.Artist"] = "Artista",
        ["Common.Artists"] = "Artistas",
        ["Common.Charts"] = "Gráficos",
        ["Common.Count"] = "Cantidad",
        ["Common.Created"] = "Creado",
        ["Common.Description"] = "Descripción",
        ["Common.Duration"] = "Duración",
        ["Common.Files"] = "Archivos",
        ["Common.Genre"] = "Género",
        ["Common.Images"] = "Imágenes",
        ["Common.LastModified"] = "Última modificación",
        ["Common.Library"] = "Biblioteca",
        ["Common.Modified"] = "Modificado",
        ["Common.NowPlaying"] = "Reproduciendo ahora",
        ["Common.Overview"] = "Resumen",
        ["Common.Playlist"] = "Lista de reproducción",
        ["Common.RadioStations"] = "Estaciones de radio",
        ["Common.Rating"] = "Calificación",
        ["Common.Release"] = "Lanzamiento",
        ["Common.Size"] = "Tamaño",
        ["Common.SortBy"] = "Ordenar por",
        ["Common.Status"] = "Estado",
        ["Common.Tracks"] = "Pistas",
        ["Common.Type"] = "Tipo",
        ["Common.Year"] = "Año",
        ["Data.AlbumCount"] = "Cantidad de álbumes",
        ["Data.AlbumTitle"] = "Título del álbum",
        ["Data.ArtistName"] = "Nombre del artista",
        ["Data.Genre"] = "Género",
        ["Data.PlayCount"] = "Reproducciones",
        ["Data.ReleaseDate"] = "Fecha de lanzamiento",
        ["Data.SongTitle"] = "Título de la canción",
        ["Data.TrackNumber"] = "Número de pista",
        ["Filters.All"] = "Todos",
        ["Filters.FilterBy"] = "Filtrar por",
        ["Filters.ShowAll"] = "Mostrar todos",
        ["Forms.Confirm"] = "Confirmar",
        ["Forms.ConfirmPassword"] = "Confirmar contraseña",
        ["Forms.CurrentPassword"] = "Contraseña actual",
        ["Forms.NewPassword"] = "Nueva contraseña",
        ["Forms.Search"] = "Buscar",
        ["Forms.SearchPlaceholder"] = "Buscar...",
        ["Messages.AreYouSure"] = "¿Estás seguro?",
        ["Messages.ConfirmAction"] = "¿Estás seguro de que quieres continuar?",
        ["Messages.DeleteConfirm"] = "¿Estás seguro de que quieres eliminar esto?",
        ["Messages.Error"] = "Error",
        ["Messages.Failed"] = "Fallido",
        ["Messages.Info"] = "Información",
        ["Messages.NoItemsFound"] = "No se encontraron elementos",
        ["Messages.NoResultsFound"] = "No se encontraron resultados",
        ["Messages.OperationCancelled"] = "Operación cancelada",
        ["Messages.OperationCompleted"] = "Operación completada",
        ["Messages.Processing"] = "Procesando...",
        ["Messages.Saved"] = "Guardado",
        ["Messages.Saving"] = "Guardando...",
        ["Messages.SuccessfullyDeleted"] = "Eliminado exitosamente",
        ["Messages.SuccessfullySaved"] = "Guardado exitosamente",
        ["Messages.SuccessfullyUpdated"] = "Actualizado exitosamente",
        ["Messages.Updating"] = "Actualizando...",
        ["Messages.Warning"] = "Advertencia",
        ["Navigation.Home"] = "Inicio",
        ["Navigation.Library"] = "Biblioteca",
        ["Navigation.Settings"] = "Configuración",
        ["Playlists.AddToPlaylist"] = "Agregar a lista de reproducción",
        ["Playlists.CreateNew"] = "Crear nueva lista",
        ["Playlists.MyPlaylists"] = "Mis listas de reproducción",
        ["Playlists.NewPlaylist"] = "Nueva lista de reproducción",
        ["Playlists.PlaylistName"] = "Nombre de la lista",
        ["Statistics.TotalAlbums"] = "Total de álbumes",
        ["Statistics.TotalArtists"] = "Total de artistas",
        ["Statistics.TotalPlays"] = "Total de reproducciones",
        ["Statistics.TotalSongs"] = "Total de canciones",
        ["Validation.EmailInvalid"] = "Dirección de correo electrónico no válida",
        ["Validation.FieldRequired"] = "Este campo es obligatorio",
        ["Validation.PasswordMismatch"] = "Las contraseñas no coinciden",
        ["Validation.PasswordTooShort"] = "La contraseña es demasiado corta",
        ["Validation.UsernameTaken"] = "Nombre de usuario ya está en uso"
    },
    ["ru-RU"] = new Dictionary<string, string>
    {
        ["Actions.Add"] = "Добавить",
        ["Actions.AddToPlaylist"] = "Добавить в плейлист",
        ["Actions.AddToQueue"] = "Добавить в очередь",
        ["Actions.Apply"] = "Применить",
        ["Actions.Back"] = "Назад",
        ["Actions.Clear"] = "Очистить",
        ["Actions.Create"] = "Создать",
        ["Actions.Download"] = "Скачать",
        ["Actions.Export"] = "Экспортировать",
        ["Actions.Import"] = "Импортировать",
        ["Actions.Next"] = "Следующий",
        ["Actions.Pause"] = "Пауза",
        ["Actions.Previous"] = "Предыдущий",
        ["Actions.Refresh"] = "Обновить",
        ["Actions.Remove"] = "Удалить",
        ["Actions.Reset"] = "Сбросить",
        ["Actions.SaveChanges"] = "Сохранить изменения",
        ["Actions.Stop"] = "Остановить",
        ["Actions.Submit"] = "Отправить",
        ["Actions.Update"] = "Обновить",
        ["Admin.Charts"] = "Графики",
        ["Admin.Libraries"] = "Библиотеки",
        ["Admin.Settings"] = "Настройки",
        ["Common.Album"] = "Альбом",
        ["Common.Albums"] = "Альбомы",
        ["Common.Artist"] = "Исполнитель",
        ["Common.Artists"] = "Исполнители",
        ["Common.Charts"] = "Графики",
        ["Common.Count"] = "Количество",
        ["Common.Created"] = "Создано",
        ["Common.Description"] = "Описание",
        ["Common.Duration"] = "Продолжительность",
        ["Common.Files"] = "Файлы",
        ["Common.Genre"] = "Жанр",
        ["Common.Images"] = "Изображения",
        ["Common.LastModified"] = "Последнее изменение",
        ["Common.Library"] = "Библиотека",
        ["Common.Modified"] = "Изменено",
        ["Common.NowPlaying"] = "Сейчас играет",
        ["Common.Overview"] = "Обзор",
        ["Common.Playlist"] = "Плейлист",
        ["Common.RadioStations"] = "Радиостанции",
        ["Common.Rating"] = "Рейтинг",
        ["Common.Release"] = "Релиз",
        ["Common.Size"] = "Размер",
        ["Common.SortBy"] = "Сортировать по",
        ["Common.Status"] = "Статус",
        ["Common.Tracks"] = "Треки",
        ["Common.Type"] = "Тип",
        ["Common.Year"] = "Год",
        ["Data.AlbumCount"] = "Количество альбомов",
        ["Data.AlbumTitle"] = "Название альбома",
        ["Data.ArtistName"] = "Имя исполнителя",
        ["Data.Genre"] = "Жанр",
        ["Data.PlayCount"] = "Количество прослушиваний",
        ["Data.ReleaseDate"] = "Дата выпуска",
        ["Data.SongTitle"] = "Название песни",
        ["Data.TrackNumber"] = "Номер трека",
        ["Filters.All"] = "Все",
        ["Filters.FilterBy"] = "Фильтровать по",
        ["Filters.ShowAll"] = "Показать все",
        ["Forms.Confirm"] = "Подтвердить",
        ["Forms.ConfirmPassword"] = "Подтвердите пароль",
        ["Forms.CurrentPassword"] = "Текущий пароль",
        ["Forms.NewPassword"] = "Новый пароль",
        ["Forms.Search"] = "Поиск",
        ["Forms.SearchPlaceholder"] = "Поиск...",
        ["Messages.AreYouSure"] = "Вы уверены?",
        ["Messages.ConfirmAction"] = "Вы уверены, что хотите продолжить?",
        ["Messages.DeleteConfirm"] = "Вы уверены, что хотите удалить это?",
        ["Messages.Error"] = "Ошибка",
        ["Messages.Failed"] = "Не удалось",
        ["Messages.Info"] = "Информация",
        ["Messages.NoItemsFound"] = "Элементы не найдены",
        ["Messages.NoResultsFound"] = "Результаты не найдены",
        ["Messages.OperationCancelled"] = "Операция отменена",
        ["Messages.OperationCompleted"] = "Операция завершена",
        ["Messages.Processing"] = "Обработка...",
        ["Messages.Saved"] = "Сохранено",
        ["Messages.Saving"] = "Сохранение...",
        ["Messages.SuccessfullyDeleted"] = "Успешно удалено",
        ["Messages.SuccessfullySaved"] = "Успешно сохранено",
        ["Messages.SuccessfullyUpdated"] = "Успешно обновлено",
        ["Messages.Updating"] = "Обновление...",
        ["Messages.Warning"] = "Предупреждение",
        ["Navigation.Home"] = "Главная",
        ["Navigation.Library"] = "Библиотека",
        ["Navigation.Settings"] = "Настройки",
        ["Playlists.AddToPlaylist"] = "Добавить в плейлист",
        ["Playlists.CreateNew"] = "Создать новый",
        ["Playlists.MyPlaylists"] = "Мои плейлисты",
        ["Playlists.NewPlaylist"] = "Новый плейлист",
        ["Playlists.PlaylistName"] = "Название плейлиста",
        ["Statistics.TotalAlbums"] = "Всего альбомов",
        ["Statistics.TotalArtists"] = "Всего исполнителей",
        ["Statistics.TotalPlays"] = "Всего прослушиваний",
        ["Statistics.TotalSongs"] = "Всего песен",
        ["Validation.EmailInvalid"] = "Недействительный адрес электронной почты",
        ["Validation.FieldRequired"] = "Это поле обязательно",
        ["Validation.PasswordMismatch"] = "Пароли не совпадают",
        ["Validation.PasswordTooShort"] = "Пароль слишком короткий",
        ["Validation.UsernameTaken"] = "Имя пользователя уже занято"
    },
    ["zh-CN"] = new Dictionary<string, string>
    {
        ["Actions.Add"] = "添加",
        ["Actions.AddToPlaylist"] = "添加到播放列表",
        ["Actions.AddToQueue"] = "添加到队列",
        ["Actions.Apply"] = "应用",
        ["Actions.Back"] = "返回",
        ["Actions.Clear"] = "清除",
        ["Actions.Create"] = "创建",
        ["Actions.Download"] = "下载",
        ["Actions.Export"] = "导出",
        ["Actions.Import"] = "导入",
        ["Actions.Next"] = "下一个",
        ["Actions.Pause"] = "暂停",
        ["Actions.Previous"] = "上一个",
        ["Actions.Refresh"] = "刷新",
        ["Actions.Remove"] = "删除",
        ["Actions.Reset"] = "重置",
        ["Actions.SaveChanges"] = "保存更改",
        ["Actions.Stop"] = "停止",
        ["Actions.Submit"] = "提交",
        ["Actions.Update"] = "更新",
        ["Admin.Charts"] = "图表",
        ["Admin.Libraries"] = "库",
        ["Admin.Settings"] = "设置",
        ["Common.Album"] = "专辑",
        ["Common.Albums"] = "专辑",
        ["Common.Artist"] = "艺术家",
        ["Common.Artists"] = "艺术家",
        ["Common.Charts"] = "图表",
        ["Common.Count"] = "计数",
        ["Common.Created"] = "创建时间",
        ["Common.Description"] = "描述",
        ["Common.Duration"] = "时长",
        ["Common.Files"] = "文件",
        ["Common.Genre"] = "流派",
        ["Common.Images"] = "图片",
        ["Common.LastModified"] = "最后修改",
        ["Common.Library"] = "库",
        ["Common.Modified"] = "修改时间",
        ["Common.NowPlaying"] = "正在播放",
        ["Common.Overview"] = "概览",
        ["Common.Playlist"] = "播放列表",
        ["Common.RadioStations"] = "电台",
        ["Common.Rating"] = "评分",
        ["Common.Release"] = "发行",
        ["Common.Size"] = "大小",
        ["Common.SortBy"] = "排序方式",
        ["Common.Status"] = "状态",
        ["Common.Tracks"] = "曲目",
        ["Common.Type"] = "类型",
        ["Common.Year"] = "年份",
        ["Data.AlbumCount"] = "专辑数量",
        ["Data.AlbumTitle"] = "专辑标题",
        ["Data.ArtistName"] = "艺术家名称",
        ["Data.Genre"] = "流派",
        ["Data.PlayCount"] = "播放次数",
        ["Data.ReleaseDate"] = "发行日期",
        ["Data.SongTitle"] = "歌曲标题",
        ["Data.TrackNumber"] = "曲目编号",
        ["Filters.All"] = "全部",
        ["Filters.FilterBy"] = "筛选方式",
        ["Filters.ShowAll"] = "显示全部",
        ["Forms.Confirm"] = "确认",
        ["Forms.ConfirmPassword"] = "确认密码",
        ["Forms.CurrentPassword"] = "当前密码",
        ["Forms.NewPassword"] = "新密码",
        ["Forms.Search"] = "搜索",
        ["Forms.SearchPlaceholder"] = "搜索...",
        ["Messages.AreYouSure"] = "您确定吗？",
        ["Messages.ConfirmAction"] = "您确定要继续吗？",
        ["Messages.DeleteConfirm"] = "您确定要删除吗？",
        ["Messages.Error"] = "错误",
        ["Messages.Failed"] = "失败",
        ["Messages.Info"] = "信息",
        ["Messages.NoItemsFound"] = "未找到项目",
        ["Messages.NoResultsFound"] = "未找到结果",
        ["Messages.OperationCancelled"] = "操作已取消",
        ["Messages.OperationCompleted"] = "操作已完成",
        ["Messages.Processing"] = "处理中...",
        ["Messages.Saved"] = "已保存",
        ["Messages.Saving"] = "保存中...",
        ["Messages.SuccessfullyDeleted"] = "删除成功",
        ["Messages.SuccessfullySaved"] = "保存成功",
        ["Messages.SuccessfullyUpdated"] = "更新成功",
        ["Messages.Updating"] = "更新中...",
        ["Messages.Warning"] = "警告",
        ["Navigation.Home"] = "首页",
        ["Navigation.Library"] = "库",
        ["Navigation.Settings"] = "设置",
        ["Playlists.AddToPlaylist"] = "添加到播放列表",
        ["Playlists.CreateNew"] = "创建新列表",
        ["Playlists.MyPlaylists"] = "我的播放列表",
        ["Playlists.NewPlaylist"] = "新播放列表",
        ["Playlists.PlaylistName"] = "播放列表名称",
        ["Statistics.TotalAlbums"] = "总专辑数",
        ["Statistics.TotalArtists"] = "总艺术家数",
        ["Statistics.TotalPlays"] = "总播放次数",
        ["Statistics.TotalSongs"] = "总歌曲数",
        ["Validation.EmailInvalid"] = "电子邮件地址无效",
        ["Validation.FieldRequired"] = "此字段为必填项",
        ["Validation.PasswordMismatch"] = "密码不匹配",
        ["Validation.PasswordTooShort"] = "密码太短",
        ["Validation.UsernameTaken"] = "用户名已被占用"
    },
    ["fr-FR"] = new Dictionary<string, string>
    {
        ["Actions.Add"] = "Ajouter",
        ["Actions.AddToPlaylist"] = "Ajouter à la liste de lecture",
        ["Actions.AddToQueue"] = "Ajouter à la file d'attente",
        ["Actions.Apply"] = "Appliquer",
        ["Actions.Back"] = "Retour",
        ["Actions.Clear"] = "Effacer",
        ["Actions.Create"] = "Créer",
        ["Actions.Download"] = "Télécharger",
        ["Actions.Export"] = "Exporter",
        ["Actions.Import"] = "Importer",
        ["Actions.Next"] = "Suivant",
        ["Actions.Pause"] = "Pause",
        ["Actions.Previous"] = "Précédent",
        ["Actions.Refresh"] = "Actualiser",
        ["Actions.Remove"] = "Supprimer",
        ["Actions.Reset"] = "Réinitialiser",
        ["Actions.SaveChanges"] = "Enregistrer les modifications",
        ["Actions.Stop"] = "Arrêter",
        ["Actions.Submit"] = "Soumettre",
        ["Actions.Update"] = "Mettre à jour",
        ["Admin.Charts"] = "Graphiques",
        ["Admin.Libraries"] = "Bibliothèques",
        ["Admin.Settings"] = "Paramètres",
        ["Common.Album"] = "Album",
        ["Common.Albums"] = "Albums",
        ["Common.Artist"] = "Artiste",
        ["Common.Artists"] = "Artistes",
        ["Common.Charts"] = "Graphiques",
        ["Common.Count"] = "Nombre",
        ["Common.Created"] = "Créé",
        ["Common.Description"] = "Description",
        ["Common.Duration"] = "Durée",
        ["Common.Files"] = "Fichiers",
        ["Common.Genre"] = "Genre",
        ["Common.Images"] = "Images",
        ["Common.LastModified"] = "Dernière modification",
        ["Common.Library"] = "Bibliothèque",
        ["Common.Modified"] = "Modifié",
        ["Common.NowPlaying"] = "En cours de lecture",
        ["Common.Overview"] = "Vue d'ensemble",
        ["Common.Playlist"] = "Liste de lecture",
        ["Common.RadioStations"] = "Stations de radio",
        ["Common.Rating"] = "Note",
        ["Common.Release"] = "Publication",
        ["Common.Size"] = "Taille",
        ["Common.SortBy"] = "Trier par",
        ["Common.Status"] = "Statut",
        ["Common.Tracks"] = "Pistes",
        ["Common.Type"] = "Type",
        ["Common.Year"] = "Année",
        ["Data.AlbumCount"] = "Nombre d'albums",
        ["Data.AlbumTitle"] = "Titre de l'album",
        ["Data.ArtistName"] = "Nom de l'artiste",
        ["Data.Genre"] = "Genre",
        ["Data.PlayCount"] = "Nombre de lectures",
        ["Data.ReleaseDate"] = "Date de sortie",
        ["Data.SongTitle"] = "Titre de la chanson",
        ["Data.TrackNumber"] = "Numéro de piste",
        ["Filters.All"] = "Tous",
        ["Filters.FilterBy"] = "Filtrer par",
        ["Filters.ShowAll"] = "Afficher tout",
        ["Forms.Confirm"] = "Confirmer",
        ["Forms.ConfirmPassword"] = "Confirmer le mot de passe",
        ["Forms.CurrentPassword"] = "Mot de passe actuel",
        ["Forms.NewPassword"] = "Nouveau mot de passe",
        ["Forms.Search"] = "Rechercher",
        ["Forms.SearchPlaceholder"] = "Rechercher...",
        ["Messages.AreYouSure"] = "Êtes-vous sûr ?",
        ["Messages.ConfirmAction"] = "Êtes-vous sûr de vouloir continuer ?",
        ["Messages.DeleteConfirm"] = "Êtes-vous sûr de vouloir supprimer ceci ?",
        ["Messages.Error"] = "Erreur",
        ["Messages.Failed"] = "Échec",
        ["Messages.Info"] = "Information",
        ["Messages.NoItemsFound"] = "Aucun élément trouvé",
        ["Messages.NoResultsFound"] = "Aucun résultat trouvé",
        ["Messages.OperationCancelled"] = "Opération annulée",
        ["Messages.OperationCompleted"] = "Opération terminée",
        ["Messages.Processing"] = "Traitement en cours...",
        ["Messages.Saved"] = "Enregistré",
        ["Messages.Saving"] = "Enregistrement...",
        ["Messages.SuccessfullyDeleted"] = "Supprimé avec succès",
        ["Messages.SuccessfullySaved"] = "Enregistré avec succès",
        ["Messages.SuccessfullyUpdated"] = "Mis à jour avec succès",
        ["Messages.Updating"] = "Mise à jour...",
        ["Messages.Warning"] = "Avertissement",
        ["Navigation.Home"] = "Accueil",
        ["Navigation.Library"] = "Bibliothèque",
        ["Navigation.Settings"] = "Paramètres",
        ["Playlists.AddToPlaylist"] = "Ajouter à la liste de lecture",
        ["Playlists.CreateNew"] = "Créer nouveau",
        ["Playlists.MyPlaylists"] = "Mes listes de lecture",
        ["Playlists.NewPlaylist"] = "Nouvelle liste de lecture",
        ["Playlists.PlaylistName"] = "Nom de la liste",
        ["Statistics.TotalAlbums"] = "Total d'albums",
        ["Statistics.TotalArtists"] = "Total d'artistes",
        ["Statistics.TotalPlays"] = "Total de lectures",
        ["Statistics.TotalSongs"] = "Total de chansons",
        ["Validation.EmailInvalid"] = "Adresse e-mail non valide",
        ["Validation.FieldRequired"] = "Ce champ est obligatoire",
        ["Validation.PasswordMismatch"] = "Les mots de passe ne correspondent pas",
        ["Validation.PasswordTooShort"] = "Le mot de passe est trop court",
        ["Validation.UsernameTaken"] = "Nom d'utilisateur déjà pris"
    },
    ["ar-SA"] = new Dictionary<string, string>
    {
        ["Actions.Add"] = "إضافة",
        ["Actions.AddToPlaylist"] = "إضافة إلى قائمة التشغيل",
        ["Actions.AddToQueue"] = "إضافة إلى قائمة الانتظار",
        ["Actions.Apply"] = "تطبيق",
        ["Actions.Back"] = "رجوع",
        ["Actions.Clear"] = "مسح",
        ["Actions.Create"] = "إنشاء",
        ["Actions.Download"] = "تحميل",
        ["Actions.Export"] = "تصدير",
        ["Actions.Import"] = "استيراد",
        ["Actions.Next"] = "التالي",
        ["Actions.Pause"] = "إيقاف مؤقت",
        ["Actions.Previous"] = "السابق",
        ["Actions.Refresh"] = "تحديث",
        ["Actions.Remove"] = "إزالة",
        ["Actions.Reset"] = "إعادة تعيين",
        ["Actions.SaveChanges"] = "حفظ التغييرات",
        ["Actions.Stop"] = "إيقاف",
        ["Actions.Submit"] = "إرسال",
        ["Actions.Update"] = "تحديث",
        ["Admin.Charts"] = "الرسوم البيانية",
        ["Admin.Libraries"] = "المكتبات",
        ["Admin.Settings"] = "الإعدادات",
        ["Common.Album"] = "الألبوم",
        ["Common.Albums"] = "الألبومات",
        ["Common.Artist"] = "الفنان",
        ["Common.Artists"] = "الفنانون",
        ["Common.Charts"] = "الرسوم البيانية",
        ["Common.Count"] = "العدد",
        ["Common.Created"] = "تاريخ الإنشاء",
        ["Common.Description"] = "الوصف",
        ["Common.Duration"] = "المدة",
        ["Common.Files"] = "الملفات",
        ["Common.Genre"] = "النوع",
        ["Common.Images"] = "الصور",
        ["Common.LastModified"] = "آخر تعديل",
        ["Common.Library"] = "المكتبة",
        ["Common.Modified"] = "تاريخ التعديل",
        ["Common.NowPlaying"] = "قيد التشغيل الآن",
        ["Common.Overview"] = "نظرة عامة",
        ["Common.Playlist"] = "قائمة التشغيل",
        ["Common.RadioStations"] = "محطات الراديو",
        ["Common.Rating"] = "التقييم",
        ["Common.Release"] = "الإصدار",
        ["Common.Size"] = "الحجم",
        ["Common.SortBy"] = "ترتيب حسب",
        ["Common.Status"] = "الحالة",
        ["Common.Tracks"] = "المقاطع",
        ["Common.Type"] = "النوع",
        ["Common.Year"] = "السنة",
        ["Data.AlbumCount"] = "عدد الألبومات",
        ["Data.AlbumTitle"] = "عنوان الألبوم",
        ["Data.ArtistName"] = "اسم الفنان",
        ["Data.Genre"] = "النوع",
        ["Data.PlayCount"] = "عدد مرات التشغيل",
        ["Data.ReleaseDate"] = "تاريخ الإصدار",
        ["Data.SongTitle"] = "عنوان الأغنية",
        ["Data.TrackNumber"] = "رقم المقطع",
        ["Filters.All"] = "الكل",
        ["Filters.FilterBy"] = "تصفية حسب",
        ["Filters.ShowAll"] = "عرض الكل",
        ["Forms.Confirm"] = "تأكيد",
        ["Forms.ConfirmPassword"] = "تأكيد كلمة المرور",
        ["Forms.CurrentPassword"] = "كلمة المرور الحالية",
        ["Forms.NewPassword"] = "كلمة المرور الجديدة",
        ["Forms.Search"] = "بحث",
        ["Forms.SearchPlaceholder"] = "بحث...",
        ["Messages.AreYouSure"] = "هل أنت متأكد؟",
        ["Messages.ConfirmAction"] = "هل أنت متأكد من أنك تريد المتابعة؟",
        ["Messages.DeleteConfirm"] = "هل أنت متأكد من أنك تريد الحذف؟",
        ["Messages.Error"] = "خطأ",
        ["Messages.Failed"] = "فشل",
        ["Messages.Info"] = "معلومات",
        ["Messages.NoItemsFound"] = "لم يتم العثور على عناصر",
        ["Messages.NoResultsFound"] = "لم يتم العثور على نتائج",
        ["Messages.OperationCancelled"] = "تم إلغاء العملية",
        ["Messages.OperationCompleted"] = "تمت العملية",
        ["Messages.Processing"] = "جاري المعالجة...",
        ["Messages.Saved"] = "تم الحفظ",
        ["Messages.Saving"] = "جاري الحفظ...",
        ["Messages.SuccessfullyDeleted"] = "تم الحذف بنجاح",
        ["Messages.SuccessfullySaved"] = "تم الحفظ بنجاح",
        ["Messages.SuccessfullyUpdated"] = "تم التحديث بنجاح",
        ["Messages.Updating"] = "جاري التحديث...",
        ["Messages.Warning"] = "تحذير",
        ["Navigation.Home"] = "الرئيسية",
        ["Navigation.Library"] = "المكتبة",
        ["Navigation.Settings"] = "الإعدادات",
        ["Playlists.AddToPlaylist"] = "إضافة إلى قائمة التشغيل",
        ["Playlists.CreateNew"] = "إنشاء جديد",
        ["Playlists.MyPlaylists"] = "قوائم التشغيل الخاصة بي",
        ["Playlists.NewPlaylist"] = "قائمة تشغيل جديدة",
        ["Playlists.PlaylistName"] = "اسم قائمة التشغيل",
        ["Statistics.TotalAlbums"] = "إجمالي الألبومات",
        ["Statistics.TotalArtists"] = "إجمالي الفنانين",
        ["Statistics.TotalPlays"] = "إجمالي مرات التشغيل",
        ["Statistics.TotalSongs"] = "إجمالي الأغاني",
        ["Validation.EmailInvalid"] = "عنوان البريد الإلكتروني غير صالح",
        ["Validation.FieldRequired"] = "هذا الحقل مطلوب",
        ["Validation.PasswordMismatch"] = "كلمات المرور غير متطابقة",
        ["Validation.PasswordTooShort"] = "كلمة المرور قصيرة جداً",
        ["Validation.UsernameTaken"] = "اسم المستخدم مستخدم بالفعل"
    }
};

// Main execution
Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine("Completing Melodee.Blazor Translations");
Console.WriteLine("=".PadRight(60, '='));

var baseResourcePath = "src/Melodee.Blazor/Resources/SharedResources.resx";
var languages = new Dictionary<string, string>
{
    ["es-ES"] = "src/Melodee.Blazor/Resources/SharedResources.es-ES.resx",
    ["ru-RU"] = "src/Melodee.Blazor/Resources/SharedResources.ru-RU.resx",
    ["zh-CN"] = "src/Melodee.Blazor/Resources/SharedResources.zh-CN.resx",
    ["fr-FR"] = "src/Melodee.Blazor/Resources/SharedResources.fr-FR.resx",
    ["ar-SA"] = "src/Melodee.Blazor/Resources/SharedResources.ar-SA.resx"
};

// Read all keys from base resource file
var baseKeys = new Dictionary<string, string>();
using (var reader = new ResXResourceReader(baseResourcePath))
{
    reader.UseResXDataNodes = true;
    foreach (System.Collections.DictionaryEntry entry in reader)
    {
        var node = (ResXDataNode)entry.Value;
        baseKeys.Add(entry.Key.ToString(), node.GetValue((ITypeResolutionService)null).ToString());
    }
}

Console.WriteLine($"\nBase resource file has {baseKeys.Count} keys");

int totalAdded = 0;

foreach (var (langCode, resourcePath) in languages)
{
    Console.WriteLine($"\nProcessing {langCode}...");
    
    // Read existing keys from language file
    var existingKeys = new HashSet<string>();
    var allEntries = new List<(string Key, string Value, string Comment)>();
    
    using (var reader = new ResXResourceReader(resourcePath))
    {
        reader.UseResXDataNodes = true;
        foreach (System.Collections.DictionaryEntry entry in reader)
        {
            var node = (ResXDataNode)entry.Value;
            var key = entry.Key.ToString();
            var value = node.GetValue((ITypeResolutionService)null).ToString();
            var comment = node.Comment ?? "";
            
            existingKeys.Add(key);
            allEntries.Add((key, value, comment));
        }
    }
    
    Console.WriteLine($"  Existing keys: {existingKeys.Count}");
    
    // Find missing keys
    var missingKeys = baseKeys.Keys.Where(k => !existingKeys.Contains(k)).ToList();
    Console.WriteLine($"  Missing keys: {missingKeys.Count}");
    
    if (missingKeys.Count == 0)
    {
        Console.WriteLine($"  No missing translations in {langCode}");
        continue;
    }
    
    // Add missing keys to entries list
    var langTranslations = translations[langCode];
    foreach (var key in missingKeys.OrderBy(k => k))
    {
        var translatedValue = langTranslations.ContainsKey(key) 
            ? langTranslations[key] 
            : baseKeys[key]; // Fallback to English if translation missing
        
        allEntries.Add((key, translatedValue, ""));
    }
    
    // Write all entries back to file (sorted by key)
    using (var writer = new ResXResourceWriter(resourcePath))
    {
        foreach (var (key, value, comment) in allEntries.OrderBy(e => e.Key))
        {
            var node = new ResXDataNode(key, value);
            if (!string.IsNullOrEmpty(comment))
            {
                node.Comment = comment;
            }
            writer.AddResource(node);
        }
        
        writer.Generate();
    }
    
    Console.WriteLine($"  Added {missingKeys.Count} translations to {langCode}");
    totalAdded += missingKeys.Count;
}

Console.WriteLine("\n" + "=".PadRight(60, '='));
Console.WriteLine($"Translation Complete! Total translations added: {totalAdded}");
Console.WriteLine("=".PadRight(60, '='));
