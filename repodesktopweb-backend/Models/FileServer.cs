﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace repodesktopweb_backend.Models;

public partial class FileServer
{
    public int Id { get; set; }

    public int Position { get; set; }

    public string NameFile { get; set; }

    public string Tagdescription { get; set; }

    public int? Estado { get; set; }

    public int? Permisos { get; set; }

    public DateTime Fecrea { get; set; }

    public string Password { get; set; }

    public string Type { get; set; }

    public int? IdFolder { get; set; }
}