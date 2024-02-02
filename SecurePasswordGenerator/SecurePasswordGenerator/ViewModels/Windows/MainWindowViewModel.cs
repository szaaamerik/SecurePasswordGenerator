using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SecurePasswordGenerator.Models;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace SecurePasswordGenerator.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    private bool _isInitialized;

    [ObservableProperty] 
    private string _applicationTitle = string.Empty;

    [ObservableProperty] 
    private bool _includeNumbers = true;

    [ObservableProperty] 
    private bool _includeSymbols = true;

    [ObservableProperty] 
    private bool _includeLowercase = true;

    [ObservableProperty] 
    private bool _includeUppercase = true;

    [ObservableProperty] 
    private bool _noDuplicateCharacters = true;

    [ObservableProperty] 
    private bool _noSequentialCharacters = true;

    [ObservableProperty] 
    private ObservableCollection<SecurePassword> _securePasswords = [];

    [ObservableProperty] 
    private bool _listHasItems;
    
    [ObservableProperty] 
    private bool _multipleEntriesSelected;
    
    [ObservableProperty] 
    private bool _noEntrySelected = true;
    
    [ObservableProperty] 
    private bool _isValidEntry;
    
    [ObservableProperty] 
    private int _selectedIndex;
    
    [ObservableProperty] 
    private ObservableCollection<SecurePassword> _selectedPasswords = [];

    [ObservableProperty] 
    private int _minLength = 10;
    
    [ObservableProperty] 
    private int _maxLength = 25;
    
    [ObservableProperty] 
    private int _quantity = 10;

    public MainWindowViewModel()
    {
        if (_isInitialized) return;
        InitializeViewModel();
    }
    
    private void InitializeViewModel()
    {
        ApplicationTitle = "Secure Password Generator";
        Generate();
        ListHasItems = true;
        _isInitialized = true;
    }

    private void Generate()
    {
        if (!IncludeNumbers && !IncludeLowercase && !IncludeUppercase && !IncludeSymbols)
        {
            const string errorMessage = "No character set selected.";
            ShowErrorMessageBox(errorMessage);
            return;
        }
        
        for (var i = 0; i < Quantity; i++)
        {
            SecurePasswords.Add(GenerateSecurePassword());
        }
    }

    private SecurePassword GenerateSecurePassword()
    {
        var secureString = GenerateSecureString();
        var strength = RatePassword(secureString);
        var color = SecurePassword.GetStrengthColorFromLevel(strength);
        return new SecurePassword
        {
            Password = secureString,
            Length = secureString.Length,
            StrengthLevel = strength,
            StrengthColor = color,
        };
    }

    private string GenerateSecureString()
    {
        const string numbers = "0123456789";
        const string symbols = "!@#$%^&*()-_=+[]{}|;:'\",.<>/?";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        var characterSet = "";
        if (IncludeNumbers)
        {
            characterSet += numbers;
        }
        if (IncludeSymbols)
        {
            characterSet += symbols;
        }
        if (IncludeLowercase)
        {
            characterSet += lowercase;
        }
        if (IncludeUppercase)
        {
            characterSet += uppercase;
        }

        var shuffledCharacterSet = new string(characterSet.ToCharArray().OrderBy(_ => Guid.NewGuid()).ToArray());
        var random = new Random();
        var length = random.Next(MinLength, MaxLength + 1);
        var result = new StringBuilder();

        for (var i = 0; i < length; i++)
        {
            var randomChar = shuffledCharacterSet[random.Next(shuffledCharacterSet.Length)];

            if (NoDuplicateCharacters && result.ToString().Contains(randomChar.ToString()))
            {
                i--;
                continue;
            }

            if (NoSequentialCharacters && i > 0)
            {
                var lastChar = result[^1];
                if (Math.Abs(lastChar - randomChar) == 1)
                {
                    i--;
                    continue;
                }
            }

            result.Append(randomChar);
        }

        return result.ToString();
    }

    private static async void ShowErrorMessageBox(string content)
    {
        const string title = "Error";
        var messageBox = new MessageBox
        {
            Title = title,
            Content = content
        };
        await messageBox.ShowDialogAsync();
    }
    
    private static SecurePassword.StrengthLevels RatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return SecurePassword.StrengthLevels.Low;
        }

        var score = 0;

        if (password.Length >= 12)
        {
            score++;
        }

        if (IncludesNumbersRegex().IsMatch(password))
        {
            score++;
        }

        if (IncludesLowercaseRegex().IsMatch(password))
        {
            score++;
        }

        if (IncludesUppercaseRegex().IsMatch(password))
        {
            score++;
        }

        if (IncludesSymbolsRegex().IsMatch(password))
        {
            score++;
        }

        if (ContainsDuplicateCharacters(password))
        {
            score--;
        }

        if (ContainsSequentialCharacters(password))
        {
            score--;
        }

        return score switch
        {
            <= 1 => SecurePassword.StrengthLevels.Low,
            2 => SecurePassword.StrengthLevels.MediumLow,
            3 => SecurePassword.StrengthLevels.Medium,
            4 => SecurePassword.StrengthLevels.MediumHigh,
            _ => SecurePassword.StrengthLevels.High
        };
    }
    
    private static bool ContainsDuplicateCharacters(string password)
    {
        for (var i = 0; i < password.Length; i++)
        {
            for (var j = i + 1; j < password.Length; j++)
            {
                if (password[i] == password[j])
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    private static bool ContainsSequentialCharacters(string password)
    {
        for (var i = 0; i < password.Length - 1; i++)
        {
            if (password[i + 1] == password[i] + 1 || password[i + 1] == password[i] - 1)
            {
                return true;
            }
        }
        
        return false;
    }

    [RelayCommand]
    private void ChangeVariable(string parameter)
    {
        switch (parameter)
        {
            case "IncludeNumbers":
            {
                IncludeNumbers = !IncludeNumbers;
                break;
            }
            case "IncludeSymbols":
            {
                IncludeSymbols = !IncludeSymbols;
                break;
            }
            case "IncludeLowercase":
            {
                IncludeLowercase = !IncludeLowercase;
                break;
            }
            case "IncludeUppercase":
            {
                IncludeUppercase = !IncludeUppercase;
                break;
            }
            case "NoDuplicates":
            {
                NoDuplicateCharacters = !NoDuplicateCharacters;
                break;
            }
            case "NoSequential":
            {
                NoSequentialCharacters = !NoSequentialCharacters;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(parameter));
            }
        }
    }

    [RelayCommand]
    private void ButtonAction(string parameter)
    {
        switch (parameter)
        {
            case "Generate":
            {
                Generate();
                ListHasItems = true;
                break;
            }
            case "Clear":
            {
                SecurePasswords.Clear();
                ListHasItems = false;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(parameter));
            }
        }
    }

    [RelayCommand]
    private void Export(string parameter)
    {
        if (SecurePasswords.Count <= 0)
        {
            return;
        }
        
        var lowercaseParameter = parameter.ToLower();
        var saveFileDialog = new SaveFileDialog
        {
            RestoreDirectory = true,
            CheckPathExists = true,
            Title = $"Generated Passwords {parameter} Export",
            DefaultExt = lowercaseParameter,
            Filter = $"{parameter} files (*.{lowercaseParameter})|*.{lowercaseParameter}|All files (*.*)|*.*"
        };

        if (saveFileDialog.ShowDialog() != true)
        {
            return;
        }

        switch (parameter)
        {
            case "CSV":
            {
                ExportToCsv(saveFileDialog.FileName);
                break;
            }
            case "TXT":
            {
                ExportToTxt(saveFileDialog.FileName);
                break;
            }
            case "Json":
            {
                ExportToJson(saveFileDialog.FileName);
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(parameter));
            }
        }
    }

    private void ExportToCsv(string filePath)
    {
        using var streamWriter = new StreamWriter(filePath);
        streamWriter.WriteLine("Password,Length,StrengthLevel");
        var list = MultipleEntriesSelected ? SelectedPasswords : SecurePasswords;
        foreach (var securePassword in list)
        {
            streamWriter.WriteLine($"{securePassword.Password},{securePassword.Length},{securePassword.StrengthLevel}");
        }
    }

    private void ExportToTxt(string filePath)
    {
        using var streamWriter = new StreamWriter(filePath);
        var list = MultipleEntriesSelected ? SelectedPasswords : SecurePasswords;
        foreach (var securePassword in list)
        {
            const string separator = "--------------------";
            streamWriter.WriteLine(separator);
            streamWriter.WriteLine($"Password: {securePassword.Password}");
            streamWriter.WriteLine($"Length: {securePassword.Length}");
            streamWriter.WriteLine($"Strength Level: {securePassword.StrengthLevel}");
            streamWriter.WriteLine(separator + "\n");
        }
    }
    
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
    private void ExportToJson(string filePath)
    {
        var list = MultipleEntriesSelected ? SelectedPasswords : SecurePasswords;
        var jsonEntries = list.Select(p => new
        {
            p.Password,
            p.Length,
            ToString = p.StrengthLevel.ToString()
        });

        var jsonString = JsonSerializer.Serialize(jsonEntries, _jsonSerializerOptions);
        File.WriteAllText(filePath, jsonString);
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        Clipboard.SetDataObject(SecurePasswords[SelectedIndex].Password);
    }

    [GeneratedRegex(@"\d")]
    private static partial Regex IncludesNumbersRegex();
    [GeneratedRegex(@"[a-z]")]
    private static partial Regex IncludesLowercaseRegex();
    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex IncludesUppercaseRegex();
    [GeneratedRegex(@"[^a-zA-Z0-9]")]
    private static partial Regex IncludesSymbolsRegex();
}