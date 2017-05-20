﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Doods.StdLibSsh.Base.Queries;
using Doods.StdLibSsh.Beans;
using Doods.StdLibSsh.Interfaces;

namespace Doods.StdLibSsh.Queries
{

    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// LC_ALL=C df -h
    /// Filesystem Size  Used Avail Use% Mounted on
    /// /dev/root       7.2G  1.2G  5.8G  17% /
    /// devtmpfs        459M     0  459M   0% /dev
    /// tmpfs           463M     0  463M   0% /dev/shm
    /// tmpfs           463M  6.2M  457M   2% /run
    /// tmpfs           5.0M  4.0K  5.0M   1% /run/lock
    /// tmpfs           463M     0  463M   0% /sys/fs/cgroup
    /// /dev/mmcblk0p1   63M   21M   42M  34% /boot
    /// </example>
    public class DiskUsageQuery : GenericQuery<IEnumerable<DiskUsageBean>>
    {
        private string DISK_USAGE_CMD = "LC_ALL=C df -h";
        private string DF_COMMAND_HEADER_START = "Filesystem";

        public DiskUsageQuery(IClientSsh client) : base(client)
        {
            CmdString = DISK_USAGE_CMD;
        }

        protected override IEnumerable<DiskUsageBean> PaseResult(string result)
        {
            return ParseDiskUsage(result);
        }
        private IEnumerable<DiskUsageBean> ParseDiskUsage(string output)
        {
            string[] lines = output.Split('\n');
            IList<DiskUsageBean> disks = new List<DiskUsageBean>();
            foreach (var line in lines)
            {
                if (line.StartsWith(DF_COMMAND_HEADER_START))
                {
                    continue;
                }

                // split string at whitespaces
                var res = Regex.Matches(line, "\\s+").Cast<Match>()
                       .ToArray(); ;

               
                if (res.Length >= 6)
                {
                    if (res.Length > 6)
                    {
                        // whitespace in mountpoint path
                        StringBuilder sb = new StringBuilder();
                        foreach (var s in res)
                        {
                            sb.Append(s);

                        }
                        sb.Append(" ");
                        disks.Add(new DiskUsageBean(res[0].Value,
                                res[1].Value, res[2].Value,
                                res[3].Value, res[4].Value, sb.ToString()));
                    }
                    else
                    {
                        disks.Add(new DiskUsageBean(res[0].Value,
                                res[1].Value, res[2].Value,
                                res[3].Value, res[4].Value,
                                res[5].Value));
                    }
                }
                else
                {
                    /* LOGGER.warn(
                             "Expected another output of df -h. Skipping line: {}",
                             line);
                             */
                }
            }
            return disks;
        }

    }
}