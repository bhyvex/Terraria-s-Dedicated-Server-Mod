﻿using System.Linq;
using TDSM.Core.Data.Models;
using OTA.Permissions;
using System.Data;
using OTA.Data.Dapper.Extensions;
using System.Collections.Generic;
using Dapper;

namespace TDSM.Core.Data
{
#if ENTITY_FRAMEWORK_6 || ENTITY_FRAMEWORK_7
    public static class OTAContextExtensions
    {
        public static IQueryable<DbPlayer> GetUser(this TContext ctx, string name)
        {
            return ctx.Players.Where(x => x.Name == name);
        }

        public static IQueryable<Group> GetUserGroups(this TContext ctx, int userId)
        {
            return ctx.Players
                .Where(x => x.Id == userId)
                .Join(ctx.PlayerGroups, pl => pl.Id, pn => pn.UserId, (pl, pg) => pg)
                .Join(ctx.Groups, pn => pn.GroupId, gr => gr.Id, (a, b) => b);
        }

        public static IQueryable<NodePermission> GetPermissionByNodeForUser(this TContext ctx, int userId, string node)
        {
            return ctx.Players
                .Where(x => x.Id == userId)
                .Join(ctx.PlayerNodes, pl => pl.Id, pn => pn.UserId, (pl, pn) => pn)
                .Join(ctx.Nodes, pn => pn.NodeId, np => np.Id, (a, b) => b)
                .Where(n => n.Node == node);
        }

        public static IQueryable<NodePermission> GetPermissionByNodeForGroup(this TContext ctx, int groupId, string node)
        {
            return ctx.Groups
                .Where(x => x.Id == groupId)
                .Join(ctx.GroupNodes, gr => gr.Id, pn => pn.GroupId, (pl, gn) => gn)
                .Join(ctx.Nodes, pn => pn.NodeId, np => np.Id, (a, b) => b)
                .Where(n => n.Node == node);
        }

        public static IQueryable<Group> GetParentForGroup(this TContext ctx, int groupId)
        {
            return ctx.Groups
                .Where(x => x.Id == groupId)
                .Join(ctx.Groups, g => g.Parent, sg => sg.Name, (a, b) => b);
            //            return ctx.Groups
            //                .Where(x => x.Id == groupId)
            //                .Select(x => ctx.Groups.Where(y => y.Name == x.Parent).FirstOrDefault());
        }

        public static bool GuestGroupHasNode(this TContext ctx, string node, Permission permission)
        {
            return ctx.Groups
                .Where(x => x.ApplyToGuests)
                .Join(ctx.GroupNodes, gr => gr.Id, pn => pn.GroupId, (pl, gn) => gn)
                .Join(ctx.Nodes, pn => pn.NodeId, np => np.Id, (a, b) => b)
                .Any(n => n.Node == node && n.Permission == permission);
        }
    }
#elif DAPPER
    public static class OTAContextExtensions
    {
        public static IEnumerable<DbPlayer> GetPlayer(this IDbConnection ctx, string name, IDbTransaction transaction = null)
        {
            return ctx.Where<DbPlayer>(new { Name = name }, transaction: transaction);
        }

        public static IEnumerable<Group> GetPlayerGroups(this IDbConnection ctx, long playerId, IDbTransaction transaction = null)
        {
            return ctx.Query<Group>($"select g.* from {TableMapper.TypeToName<DbPlayer>()} p " +
                $"inner join {TableMapper.TypeToName<PlayerGroup>()} pg on p.Id = pg.UserId " +
                $"inner join {TableMapper.TypeToName<Group>()} g on pg.GroupId = g.Id " +
                "where p.Id = @PlayerId", new { PlayerId = playerId }, transaction: transaction);
        }

        public static IEnumerable<PermissionNode> GetPermissionByNodeForPlayer(this IDbConnection ctx, long playerId, string node, IDbTransaction transaction = null)
        {
            return ctx.Query<PermissionNode>($"select nd.* from {TableMapper.TypeToName<DbPlayer>()} p " +
                $"inner join {TableMapper.TypeToName<PlayerNode>()} pn on p.Id = pn.UserId " +
                $"inner join {TableMapper.TypeToName<PermissionNode>()} nd on pn.NodeId = nd.Id " +
                "where p.Id = @PlayerId and nd.Node = @Node", new { PlayerId = playerId, Node = node }, transaction: transaction);
        }

        public static IEnumerable<PermissionNode> GetPermissionByNodeForGroup(this IDbConnection ctx, long groupId, string node, IDbTransaction transaction = null)
        {
            return ctx.Query<PermissionNode>($"select nd.* from {TableMapper.TypeToName<Group>()} g " +
                $"inner join {TableMapper.TypeToName<GroupNode>()} gn on g.Id = pn.GroupId " +
                $"inner join {TableMapper.TypeToName<PermissionNode>()} nd on gn.NodeId = nd.Id " +
                "where g.Id = @GroupId and nd.Node = @Node", new { GroupId = groupId, Node = node }, transaction: transaction);
        }

        public static IEnumerable<Group> GetParentForGroup(this IDbConnection ctx, long groupId, IDbTransaction transaction = null)
        {
            return ctx.Query<Group>($"select p.* from {TableMapper.TypeToName<Group>()} g " +
                $"inner join {TableMapper.TypeToName<Group>()} p on g.Parent = p.Name " +
                "where g.Id = @GroupId", new { GroupId = groupId }, transaction: transaction);
        }

        public static bool GuestGroupHasNode(this IDbConnection ctx, string node, Permission permission, IDbTransaction transaction = null)
        {
            return ctx.ExecuteScalar<long>($"select count(nd.Id) from {TableMapper.TypeToName<Group>()} g " +
                $"inner join {TableMapper.TypeToName<GroupNode>()} gn on g.Id = gn.GroupId " +
                $"inner join {TableMapper.TypeToName<PermissionNode>()} nd on gn.NodeId = nd.Id " +
                "where g.ApplyToGuests = 1 and nd.Node = @Node and nd.Permission = @Permission", new { Node = node, Permission = (int)permission }, transaction: transaction) > 0L;
        }
    }
#endif
}

